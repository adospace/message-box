using MessageBox.Messages;
using System.Collections.Concurrent;

namespace MessageBox.Server.Implementation
{
    internal class Bus : IBus, IMessageSink, IBusServer
    {
        private readonly ConcurrentDictionary<string, IExchange> _exchanges = new(StringComparer.InvariantCultureIgnoreCase);

        private readonly ConcurrentDictionary<Guid, IQueue> _queues = new();

        private readonly ITransport _transport;

        public Bus(ITransportFactory transportFactory)
        {
            _transport = transportFactory.Create();
        }

        public IExchange GetOrCreateExchange(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
            }

            return _exchanges.GetOrAdd(key, _ =>
            {
                var board = new Exchange(_);
                board.Start();
                return board;
            });
        }

        public IQueue GetOrCreateQueue(Guid id)
        {
            return _queues.GetOrAdd(id, _ => new Queue(_));
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            await _transport.Run(cancellationToken);
        }

        public async Task Stop(CancellationToken cancellationToken)
        {
            await _transport.Stop(cancellationToken);

            foreach (var board in _exchanges.ToArray())
            {
                board.Value.Stop();
            }
        }

        public async Task OnReceivedMessage(IMessage message, CancellationToken cancellationToken)
        {
            switch (message)
            {
                case ISubscribeQueuedMessage subscribeToExchangeMessage:
                {
                    var exchange = GetOrCreateExchange(subscribeToExchangeMessage.ExchangeName);
                    exchange.Subscribe(GetOrCreateQueue(subscribeToExchangeMessage.SourceQueueId));
                    break;
                }
                case IPublishEventMessage publishEventMessage:
                {
                    var exchange = GetOrCreateExchange(publishEventMessage.ExchangeName);
                    await exchange.OnReceivedMessage(message, cancellationToken);
                    break;
                }
                case ICallMessage callMessage:
                {
                    var exchange = GetOrCreateExchange(callMessage.ExchangeName);
                    await exchange.OnReceivedMessage(message, cancellationToken);
                    break;
                }
                case IReplyMessage queuedMessage:
                {
                    _queues.TryGetValue(queuedMessage.ReplyToBoxId, out var queue);
                    if (queue != null)
                    {
                        await queue.OnReceivedMessage(message, cancellationToken);
                    }
                    break;
                }
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
