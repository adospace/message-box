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

        public async Task OnReceivedMessage(Message message, CancellationToken cancellationToken)
        {
            if (message.BoardKey != null)
            {
                var board = GetOrCreateExchange(message.BoardKey);

                if (message.Payload == null && message.PayloadType == null)
                {
                    if (message.ReplyToBoxId == null)
                    {
                        throw new InvalidOperationException();
                    }

                    board.Subscribe(GetOrCreateQueue(message.ReplyToBoxId.Value));
                }
                else
                {
                    await board.OnReceivedMessage(message, cancellationToken);
                }

            }
            else if (message.ReplyToBoxId != null)
            {
                var box = GetOrCreateQueue(message.ReplyToBoxId.Value);

                await box.OnReceivedMessage(message, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
