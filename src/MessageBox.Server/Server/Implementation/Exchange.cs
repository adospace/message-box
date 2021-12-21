using MessageBox.Messages;
using Nito.AsyncEx;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace MessageBox.Server.Implementation
{
    internal class Exchange : IExchange
    {
        private readonly Channel<IMessage> _outgoingMessages = Channel.CreateUnbounded<IMessage>();

        private readonly ConcurrentDictionary<Guid, WeakReference<IQueue>> _subscribers = new();

        private readonly ConcurrentQueue<WeakReference<IQueue>> _subscribersQueue = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncAutoResetEvent _subscribersListIsEmptyEvent = new();

        public Exchange(string key)
        {
            Key = key;
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

                    while (true)
                    {
                        if (message is IPublishEventMessage publishEventMessage)
                        {
                            foreach (var subsriber in _subscribers.ToArray())
                            {
                                if (subsriber.Value.TryGetTarget(out var queue))
                                {
                                    await queue.OnReceivedMessage(message, _cancellationTokenSource.Token);
                                }
                                else
                                {
                                    _subscribers.Remove(subsriber.Key, out var _);
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
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
