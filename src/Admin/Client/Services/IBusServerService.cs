using MessageBox.Server.Tcp.Host.Shared;

namespace MessageBox.Server.Tcp.Host.Client.Services;

public interface IBusServerService
{
    Task<IEnumerable<QueueControlModel>> GetQueues();

    Task<IEnumerable<ExchangeControModel>> GetExchanges();
}