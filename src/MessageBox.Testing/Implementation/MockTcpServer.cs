using MessageBox.Server;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MessageBox.Testing.Implementation
{
    internal class MockTcpServer
    {
        private readonly Channel<Message> _incomingMessages = Channel.CreateUnbounded<Message>();
        private readonly ConcurrentDictionary<Guid, MockTcpConnectedClient> _clients = new();
        private readonly IBusServer _bus;
        private readonly IMessageSink _messageSink;

        public MockTcpServer(IBusServer bus, IMessageSink messageSink)
        {
            _bus = bus;
            _messageSink = messageSink;
        }

        public void OnClientConnected(MockTcpClient client)
        {
            Guid boxId = Guid.NewGuid();
            
            var box = _bus.GetBox(boxId);

            _clients[boxId] = new MockTcpConnectedClient(_messageSink, box, client);
            _clients[boxId].Start();
        }
    }
}
