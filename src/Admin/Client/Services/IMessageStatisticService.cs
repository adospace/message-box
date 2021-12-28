using MessageBox.Server.Tcp.Host.Shared;

namespace MessageBox.Server.Tcp.Host.Client.Services;

public interface IMessageStatisticService
{
    Task<ServerMessageCountStatistic> GetServerMessageStatistic();
}