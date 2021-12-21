using MessageBox.Messages;
using Microsoft.Extensions.DependencyInjection;

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
                await connectedClient.OnReceivedMessage(messageToSend, cancellationToken);
            }
        }

        public Task Stop(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        internal Task OnReceiveMessageFromServer(IMessage message, CancellationToken token)
        {
            var sink = _serviceProvider.GetRequiredService<IMessageSink>();
            
            return sink.OnReceivedMessage(message, token);
        }
    }
}
