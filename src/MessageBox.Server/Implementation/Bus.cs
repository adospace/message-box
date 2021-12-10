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

        private readonly ConcurrentDictionary<Guid, ITransport> _transports = new();

        private readonly ITransportFactory _transportFactory;

        public Bus(ITransportFactory transportFactory)
        {
            _transportFactory = transportFactory;
        }

        public IBoard GetBoard(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException($"'{nameof(key)}' cannot be null or whitespace.", nameof(key));
            }

            return _boards.GetOrAdd(key, key => new Board(key));
        }

        public IBox GetBox(Guid id)
        {
            return _boxes.GetOrAdd(id, id => 
            {
                var box = new Box(this, id);
                
                _transports.GetOrAdd(box.Id, key => _transportFactory.Create(box, this));

                return box;
            });
        }

        public void Start()
        {
            foreach (var board in _boards.ToArray())
            {
                board.Value.Start();
            }

            foreach (var transport in _transports.ToArray())
            {
                transport.Value.Start();
            }
        }

        public void Stop()
        {
            foreach (var board in _boards.ToArray())
            {
                board.Value.Stop();
            }

            foreach (var transport in _transports.ToArray())
            {
                transport.Value.Stop();
            }
        }

        public async Task OnReceivedMessage(Message message, CancellationToken cancellationToken)
        {
            IMessageSink? sink;

            if (message.BoardKey != null)
            {
                sink = GetBoard(message.BoardKey);
            }
            else if (message.DestinationBoxId != null)
            {
                sink = GetBox(message.DestinationBoxId.Value);
            }
            else
            {
                throw new InvalidOperationException();
            }

            await sink.OnReceivedMessage(message, cancellationToken);
        }
    }
}
