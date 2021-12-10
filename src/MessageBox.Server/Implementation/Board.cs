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

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public Board(string key)
        {
            Key = key;
        }

        public string Key { get; }

        public async Task OnReceivedMessage(Message message, CancellationToken cancellationToken)
        {
            await _outgoingMessages.Writer.WriteAsync(message, cancellationToken);
        }

        public void Subsscribe(IBox box)
        {
            _subscribers[box.Id] = new WeakReference<IBox>(box);
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
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
