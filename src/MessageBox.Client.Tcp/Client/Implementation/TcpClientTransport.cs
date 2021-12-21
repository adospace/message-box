using MessageBox.Tcp;
using Microsoft.Extensions.DependencyInjection;
using System.Buffers;
using System.Net.Sockets;

namespace MessageBox.Client.Implementation
{
    internal class TcpClientTransport : TcpConnection, ITransport
    {
        private readonly TcpTransportOptions _options;

        public TcpClientTransport(IServiceProvider serviceProvider, TcpTransportOptions options)
            :base(serviceProvider)
        {
            _options = options;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                using var tcpClient = new TcpClient();

                try
                {
                    await tcpClient.ConnectAsync(_options.ServerEndPoint, cancellationToken);
                }
                catch (Exception)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.Delay(10000, cancellationToken);
                    }

                    continue;
                }

                await RunConnectionLoop(
                    connectedSocket: tcpClient.Client, 
                    messageSource: _serviceProvider.GetRequiredService<IMessageSource>(),
                    messageSink: _serviceProvider.GetRequiredService<IMessageSink>(),
                    cancellationToken);
            }
        }

        public Task Stop(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override void OnConnectionLoopEnded()
        {
            
        }
    }
}
