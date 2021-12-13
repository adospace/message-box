using MessageBox.Tcp;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Client.Tcp.Implementation
{
    internal class TcpClientTransport : TcpConnection, ITransport
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TcpTransportOptions _options;

        public TcpClientTransport(IServiceProvider serviceProvider, TcpTransportOptions options)
        {
            _serviceProvider = serviceProvider;
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

                var messageSink = _serviceProvider.GetRequiredService<IMessageSink>();
                var messageSource = _serviceProvider.GetRequiredService<IMessageSource>();

                await RunConnectionLoop(tcpClient.Client, messageSink, messageSource, cancellationToken);
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
