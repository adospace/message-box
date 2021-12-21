using MessageBox.Messages;
using System.Collections.Concurrent;

namespace MessageBox.Server.Implementation
{
    internal class Bus : IBus, IMessageSink, IBusServer
    {
        private readonly ConcurrentDictionary<string, IExchange> _boards = new(StringComparer.InvariantCultureIgnoreCase);

        private readonly ConcurrentDictionary<Guid, IQueue> _boxes = new();

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

            return _boards.GetOrAdd(key, _ =>
            {
                var board = new Exchange(_);
                board.Start();
                return board;
            });
        }

        public IQueue GetOrCreateQueue(Guid id)
        {
            return _boxes.GetOrAdd(id, _ => new Queue(_));
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            await _transport.Run(cancellationToken);
        }

        public async Task Stop(CancellationToken cancellationToken)
        {
            await _transport.Stop(cancellationToken);

            foreach (var board in _boards.ToArray())
            {
                board.Value.Stop();
            }
        }

        public async Task OnReceivedMessage(IMessage message, CancellationToken cancellationToken)
        {
            if (message is ISubscribeQueuedMessage subscribeToExchangeMessage)
            { 
                var exchange = GetOrCreateExchange(subscribeToExchangeMessage.ExchangeName);
                exchange.Subscribe(GetOrCreateQueue(subscribeToExchangeMessage.SourceQueueId));
            }
            else if (message is IPublishEventMessage publishEventMessage)
            {
                var exchange = GetOrCreateExchange(publishEventMessage.ExchangeName);
                await exchange.OnReceivedMessage(message, cancellationToken);
            }
            else if (message is ICallMessage callMessage)
            {
                var exchange = GetOrCreateExchange(callMessage.ExchangeName);
                await exchange.OnReceivedMessage(message, cancellationToken);
            }
            else if (message is IReplyMessage queuedMessage)
            {
                var queue = GetOrCreateQueue(queuedMessage.ReplyToBoxId);
                await queue.OnReceivedMessage(message, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
