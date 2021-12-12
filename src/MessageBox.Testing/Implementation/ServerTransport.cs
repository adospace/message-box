using MessageBox.Server;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Testing.Implementation
{
    internal class ServerTransport : ITransport
    {
        private readonly ConcurrentDictionary<Guid, ConnectionFromClient> _clients = new();
        private readonly IServiceProvider _serviceProvider;

        public ServerTransport(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            Instance = this;
        }

        public Task Run(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task Stop(CancellationToken cancellationToken)
        {
            foreach (var client in _clients.ToArray())
            {
                client.Value.Stop();
            }

            return Task.CompletedTask;
        }

        public static ServerTransport? Instance { get; private set; }

        public ConnectionFromClient OnClientConnected(ClientTransport client)
        {
            Guid boxId = Guid.NewGuid();

            var bus = _serviceProvider.GetRequiredService<IBusServer>();
            var messageSink = _serviceProvider.GetRequiredService<IMessageSink>();

            var box = bus.GetOrCreateBox(boxId);

            _clients[boxId] = new ConnectionFromClient(messageSink, box, client);
            _clients[boxId].Start();

            return _clients[boxId];
        }

    }
}
