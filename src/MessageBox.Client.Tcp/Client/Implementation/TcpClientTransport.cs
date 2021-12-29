using MessageBox.Tcp;
using Microsoft.Extensions.DependencyInjection;
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

        public async Task Run(Func<CancellationToken, Task>? onConnectionSucceed, Func<CancellationToken, Task>? onConnectionEnded, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    using var tcpClient = new TcpClient();

                    try
                    {
                        await tcpClient.ConnectAsync(_options.ServerEndPoint, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            await Task.Delay(10000, cancellationToken);
                        }

                        continue;
                    }

                    if (onConnectionSucceed != null)
                    {
                        await onConnectionSucceed(cancellationToken);
                    }

                    await RunConnectionLoop(
                        connectedSocket: tcpClient.Client, 
                        messageSource: _serviceProvider.GetRequiredService<IMessageSource>(),
                        messageSink: _serviceProvider.GetRequiredService<IMessageSink>(),
                        cancellationToken);
                
                    if (onConnectionEnded != null)
                    {
                        await onConnectionEnded(cancellationToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
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
