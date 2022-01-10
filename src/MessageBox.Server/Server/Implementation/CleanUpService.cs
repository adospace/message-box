using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageBox.Server.Implementation;

public class CleanUpService : BackgroundService
{
    private readonly IBusServerControl _busServerControl;
    private readonly ILogger<CleanUpService> _logger;
    private readonly TimeSpan _keepAliveTimeout;

    public CleanUpService(IBusServerControl busServerControl, ILogger<CleanUpService> logger, ICleanUpServiceOptions? options = null)
    {
        _busServerControl = busServerControl;
        _logger = logger;
        _keepAliveTimeout = options?.KeepAliveTimeout ?? TimeSpan.FromSeconds(10);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000, cancellationToken);

                var queueControls = _busServerControl.GetQueues();

                foreach (var queueControl in queueControls)
                {
                    if (!await queueControl.IsAlive(_keepAliveTimeout, cancellationToken))
                    {
                        _logger.LogDebug("Deleting Queue '{Name}' ({Id})", queueControl.Name, queueControl.Id);
                        _busServerControl.DeleteQueue(queueControl.Id);
                    }
                }

                var exchangeControls = _busServerControl.GetExchanges();

                foreach (var exchangeControl in exchangeControls)
                {
                    if (!exchangeControl.IsAlive(_keepAliveTimeout))
                    {
                        _logger.LogDebug("Deleting Exchange '{Key}'", exchangeControl.Key);
                        _busServerControl.DeleteExchange(exchangeControl.Key);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}