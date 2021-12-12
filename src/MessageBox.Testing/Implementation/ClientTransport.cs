using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Testing.Implementation
{
    internal class ClientTransport : ITransport
    {
        private readonly IServiceProvider _serviceProvider;

        public ClientTransport(IServiceProvider serviceProvider)
        { 
            _serviceProvider = serviceProvider;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            var connectedClient = ServerTransport.Instance?.OnClientConnected(this) ?? throw new InvalidOperationException();
            var source = _serviceProvider.GetRequiredService<IMessageSource>();

            while (!cancellationToken.IsCancellationRequested)
            {
                var messageToSend = await source.GetNextMessageToSend(cancellationToken);
                await connectedClient.ReceiveMessageFromClient(messageToSend, cancellationToken);
            }
        }

        public Task Stop(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        internal Task OnReceiveMessageFromServer(Message message, CancellationToken token)
        {
            var sink = _serviceProvider.GetRequiredService<IMessageSink>();
            
            return sink.OnReceivedMessage(message, token);
        }
    }
}
