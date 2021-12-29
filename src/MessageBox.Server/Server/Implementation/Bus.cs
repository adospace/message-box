using MessageBox.Messages;
using System.Collections.Concurrent;

namespace MessageBox.Server.Implementation
{
    internal class Bus : IBus, IMessageSink, IBusServer, IBusServerControl
    {
        private readonly IMessageFactory _messageFactory;
        private readonly ConcurrentDictionary<string, IExchange> _exchanges = new(StringComparer.InvariantCultureIgnoreCase);

        private readonly ConcurrentDictionary<Guid, IQueue> _queues = new();

        private readonly ITransport _transport;

        public Bus(ITransportFactory transportFactory, IMessageFactory messageFactory)
        {
            _messageFactory = messageFactory;
            _transport = transportFactory.Create();
        }

        public IQueue? GetQueue(Guid id)
        {
            _queues.TryGetValue(id, out var queue);
            return queue;
        }

        public IQueue CreateQueue(Guid id, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
            }
            
            return _queues.GetOrAdd(id, _ => new Queue(_, key, _messageFactory));
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

        public async Task Run(CancellationToken cancellationToken)
        {
            await _transport.Run(cancellationToken: cancellationToken);
        }

        public async Task Stop(CancellationToken cancellationToken)
        {
            await _transport.Stop(cancellationToken);

            foreach (var board in _exchanges.ToArray())
            {
                board.Value.Stop();
            }
            
            foreach (var board in _queues.ToArray())
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
                    var queue = GetQueue(subscribeToExchangeMessage.SourceQueueId);
                    if (queue != null)
                    {
                        exchange.Subscribe(queue);    
                    }
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
                case ISetQueueNameQueuedMessage setQueueNameQueuedMessage:
                {
                    _queues.TryGetValue(setQueueNameQueuedMessage.QueueId, out var queue);
                    if (queue != null)
                    {
                        await queue.OnReceivedMessage(message, cancellationToken);
                    }
                    break;
                }
                case IKeepAliveMessage keepAliveMessage:
                {
                    _queues.TryGetValue(keepAliveMessage.QueueId, out var queue);
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

        public IReadOnlyList<IQueueControl> GetQueues()
        {
            return _queues.Values.Cast<IQueueControl>().ToList();
        }

        public IReadOnlyList<IExchangeControl> GetExchanges()
        {
            return _exchanges.Values.Cast<IExchangeControl>().ToList();
        }

        public void DeleteQueue(Guid id)
        {
            if (_queues.Remove(id, out var queue))
            {
                queue.Stop();
            }
        }

        public void DeleteExchange(string name)
        {
            if (_exchanges.Remove(name, out var exchange))
            {
                exchange.Stop();    
            }
        }
    }
}
