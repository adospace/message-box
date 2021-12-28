using Microsoft.Extensions.Hosting;

namespace MessageBox.Server.Implementation;

public class CleanUpService : BackgroundService
{
    private readonly IBusServerControl _busServerControl;
    private readonly TimeSpan _keepAliveTimeout;

    public CleanUpService(IBusServerControl busServerControl, ICleanUpServiceOptions? options = null)
    {
        _busServerControl = busServerControl;
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
                        _busServerControl.DeleteQueue(queueControl.Id);
                    }
                }

                var exchangeControls = _busServerControl.GetExchanges();

                foreach (var exchangeControl in exchangeControls)
                {
                    if (!exchangeControl.IsAlive(_keepAliveTimeout))
                    {
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