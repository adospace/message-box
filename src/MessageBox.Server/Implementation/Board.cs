using Nito.AsyncEx;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MessageBox.Server.Implementation
{
    internal class Board : IBoard
    {
        private readonly Channel<Message> _outgoingMessages = Channel.CreateUnbounded<Message>();

        private readonly ConcurrentDictionary<Guid, WeakReference<IBox>> _subscribers = new();

        private readonly ConcurrentQueue<WeakReference<IBox>> _subscribersQueue = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly AsyncAutoResetEvent _subscribersListIsEmptyEvent = new();

        public Board(string key)
        {
            Key = key;
        }

        public string Key { get; }

        public async Task OnReceivedMessage(Message message, CancellationToken cancellationToken)
        {
            await _outgoingMessages.Writer.WriteAsync(message, cancellationToken);
        }

        public void Subscribe(IBox box)
        {
            var refToBox = new WeakReference<IBox>(box);
            _subscribers[box.Id] = refToBox;
            _subscribersQueue.Enqueue(refToBox);

            _subscribersListIsEmptyEvent.Set();
        }

        public async void Start()
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
                    if (message.IsEvent)
                    {
                        foreach (var subsriber in _subscribers.ToArray())
                        {
                            if (subsriber.Value.TryGetTarget(out var box))
                            {
                                await box.OnReceivedMessage(message, _cancellationTokenSource.Token);
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

                            if (!boxReference.TryGetTarget(out var box))
                                continue;

                            await box.OnReceivedMessage(message, _cancellationTokenSource.Token);

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

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
