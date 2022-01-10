using MessageBox.Messages;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace MessageBox.Server.Implementation
{
    internal class Bus : IBus, IMessageSink, IBusServer, IBusServerControl
    {
        private readonly IMessageFactory _messageFactory;
        private readonly ILogger<Bus> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ConcurrentDictionary<string, IExchange> _exchanges = new(StringComparer.InvariantCultureIgnoreCase);

        private readonly ConcurrentDictionary<Guid, IQueue> _queues = new();

        private readonly ITransport _transport;

        public Bus(ITransportFactory transportFactory, IMessageFactory messageFactory, ILogger<Bus> logger, ILoggerFactory loggerFactory)
        {
            _messageFactory = messageFactory;
            _logger = logger;
            _loggerFactory = loggerFactory;
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

            _logger.LogDebug("Creating Queue '{Name}' ({Id})", key, id);

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
                _logger.LogDebug("Creating Exchange '{Key}'", key);

                var board = new Exchange(_, _loggerFactory.CreateLogger<Exchange>());
                board.Start();
                return board;
            });
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            _logger.LogTrace("Start bus");

            await _transport.Run(cancellationToken: cancellationToken);
        }

        public async Task Stop(CancellationToken cancellationToken)
        {
            using var _ = _logger.BeginScope("Stop bus");

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
                _logger.LogDebug("Removing Queue '{Name}' ({Id})", queue.Name, id);
                queue.Stop();
            }
        }

        public void DeleteExchange(string name)
        {
            if (_exchanges.Remove(name, out var exchange))
            {
                _logger.LogDebug("Removing Exchange '{Key}'", exchange.Key);
                exchange.Stop();    
            }
        }
    }
}
