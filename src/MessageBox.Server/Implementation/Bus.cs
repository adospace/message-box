using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Server.Implementation
{
    internal class Bus : IBus, IMessageSink, IBusServer
    {
        private readonly ConcurrentDictionary<string, IBoard> _boards = new(StringComparer.InvariantCultureIgnoreCase);

        private readonly ConcurrentDictionary<Guid, IBox> _boxes = new();

        private readonly ITransport _transport;

        public Bus(ITransportFactory transportFactory)
        {
            _transport = transportFactory.Create();
        }

        public IBoard GetOrCreateBoard(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
            }

            return _boards.GetOrAdd(key, key =>
            {
                var board = new Board(key);
                board.Start();
                return board;
            });
        }

        public IBox GetOrCreateBox(Guid id)
        {
            return _boxes.GetOrAdd(id, id => new Box(id));
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
                var board = GetOrCreateBoard(message.BoardKey);

                if (message.Payload == null && message.PayloadType == null)
                {
                    if (message.ReplyToBoxId == null)
                    {
                        throw new InvalidOperationException();
                    }

                    board.Subscribe(GetOrCreateBox(message.ReplyToBoxId.Value));
                }
                else
                {
                    await board.OnReceivedMessage(message, cancellationToken);
                }

            }
            else if (message.ReplyToBoxId != null)
            {
                var box = GetOrCreateBox(message.ReplyToBoxId.Value);

                await box.OnReceivedMessage(message, cancellationToken);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
