using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace MessageBox.Server.Implementation
{
    internal class TcpServerTransport : ITransport
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TcpTransportOptions _options;
        private readonly ILogger<TcpServerTransport> _logger;
        private readonly ConcurrentDictionary<Guid, ConnectionFromClient> _clients = new();

        public TcpServerTransport(IServiceProvider serviceProvider, TcpTransportOptions options)
        {
            _serviceProvider = serviceProvider;
            _logger = serviceProvider.GetRequiredService<ILogger<TcpServerTransport>>();
            _options = options;
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            await ConnectionLoop(cancellationToken);
        }

        public Task Stop(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task ConnectionLoop(CancellationToken cancellationToken)
        {
            TcpListener? tcpListener = null;

            while (tcpListener == null && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    tcpListener = new TcpListener(_options.ServerEndPoint);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to bind to local address {ServerEndPoint}, waiting 60000ms before try again", _options.ServerEndPoint);

                    await Task.Delay(60000, cancellationToken);
                }
            }

            if (tcpListener == null)
            {
                return;
            }

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Start listening on {ServerEndPoint}", _options.ServerEndPoint);
                    tcpListener.Start();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to listen on {ServerEndPoint}", _options.ServerEndPoint);
                    await Task.Delay(60000, cancellationToken);
                    continue;
                }

                while (!cancellationToken.IsCancellationRequested)
                {
                    Socket socketConnectedToClient;
                    try
                    {
                        socketConnectedToClient = await tcpListener.AcceptSocketAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }

                    _logger.LogInformation("Connection accepted from {RemoteEndPointIp}, begin connection loop", socketConnectedToClient.RemoteEndPoint?.ToString());

                    socketConnectedToClient.ReceiveTimeout = _options.SocketReceiveTimeout;
                    socketConnectedToClient.SendTimeout = _options.SocketSendTimeout;

                    var queueId = Guid.NewGuid();

                    var bus = _serviceProvider.GetRequiredService<IBusServer>();
                    var messageSink = _serviceProvider.GetRequiredService<IMessageSink>();

                    var queue = bus.GetOrCreateQueue(queueId);

                    _clients[queueId] = new ConnectionFromClient(_serviceProvider, queue, id => _clients.TryRemove(id, out var _));

                    _clients[queueId].StartConnectionLoop(socketConnectedToClient, queue, messageSink, cancellationToken);
                }
            }

            tcpListener.Stop();
        }

    }
}
