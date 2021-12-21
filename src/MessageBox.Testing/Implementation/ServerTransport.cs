using MessageBox.Messages;
using MessageBox.Server;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

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
            var queueId = Guid.NewGuid();

            var bus = _serviceProvider.GetRequiredService<IBusServer>();
            var messageSink = _serviceProvider.GetRequiredService<IMessageSink>();
            var messageFactory = _serviceProvider.GetRequiredService<IMessageFactory>();

            var queue = bus.GetOrCreateQueue(queueId);

            _clients[queueId] = new ConnectionFromClient(messageFactory, messageSink, queue, client);
            _clients[queueId].Start();

            return _clients[queueId];
        }

    }
}
