using MessageBox.Messages;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace MessageBox.Server.Implementation
{
    internal class Exchange : IExchange, IExchangeControl
    {
        private readonly Channel<IMessage> _outgoingMessages = Channel.CreateUnbounded<IMessage>();

        private readonly ConcurrentDictionary<Guid, WeakReference<IQueue>> _subscribers = new();

        private readonly ConcurrentQueue<WeakReference<IQueue>> _subscribersQueue = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncAutoResetEvent _subscribersListIsEmptyEvent = new();
        
        private readonly ILogger<Exchange> _logger;
        
        private DateTime _lastReceivedMessageTimeStamp = DateTime.UtcNow;

        private int _messageCount;

        public Exchange(string key, ILogger<Exchange> logger)
        {
            Key = key;
            _logger = logger;
        }

        public string Key { get; }

        public async Task OnReceivedMessage(IMessage message, CancellationToken cancellationToken)
        {
            await _outgoingMessages.Writer.WriteAsync(message, cancellationToken);
        }

        public void Subscribe(IQueue queue)
        {
            var refToBox = new WeakReference<IQueue>(queue);
            _subscribers[queue.Id] = refToBox;
            _subscribersQueue.Enqueue(refToBox);

            _subscribersListIsEmptyEvent.Set();

            _logger.LogDebug("Exchange {Key} Subscribed Queue '{Name}' ({Id})", Key, queue.Name, queue.Id);
        }

        public async void Start()
        {
            try
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    var message = await _outgoingMessages.Reader.ReadAsync(_cancellationTokenSource.Token);

                    if (_cancellationTokenSource.IsCancellationRequested)
                    {
                        break;
                    }

                    _lastReceivedMessageTimeStamp = DateTime.UtcNow;
                    _messageCount++;

                    while (true)
                    {
                        if (message is IPublishEventMessage)
                        {
                            foreach (var (subscriberKey, queueReference) in _subscribers.ToArray())
                            {
                                if (queueReference.TryGetTarget(out var queue))
                                {
                                    await queue.OnReceivedMessage(message, _cancellationTokenSource.Token);
                                }
                                else
                                {
                                    _subscribers.Remove(subscriberKey, out var _);
                                }
                            }

                            if (!_subscribers.IsEmpty)
                            {
                                break;
                            }
                        }
                        else
                        {
                            while (true)
                            {
                                if (!_subscribersQueue.TryDequeue(out var boxReference))
                                    break;

                                if (!boxReference.TryGetTarget(out var queue))
                                    continue;

                                await queue.OnReceivedMessage(message, _cancellationTokenSource.Token);

                                _subscribersQueue.Enqueue(boxReference);
                                break;
                            }

                            if (!_subscribersQueue.IsEmpty)
                            {
                                break;
                            }
                        }

                        await _subscribersListIsEmptyEvent.WaitAsync(_cancellationTokenSource.Token);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ChannelClosedException)
            {
            }
        }

        public void Stop()
        {
            _outgoingMessages.Writer.Complete();
            _cancellationTokenSource.Cancel();
        }
        
        public int GetTotalMessageCount()
        {
            return _messageCount;
        }

        public int GetCurrentMessageCount()
        {
            return _outgoingMessages.Reader.Count;
        }

        public IReadOnlyList<IQueueControl> GetSubscribers()
        {
            return _subscribers.ToList().Select(_ =>
            {
                _.Value.TryGetTarget(out var queue);
                return queue;
            })
            .Where(_=>_ != null)
            .Cast<IQueueControl>()
            .ToList();
        }

        public bool IsAlive(TimeSpan keepAliveTimeout)
        {
            return GetSubscribers().Count > 0 || (DateTime.UtcNow - _lastReceivedMessageTimeStamp) <= keepAliveTimeout;
        }
    }
}
