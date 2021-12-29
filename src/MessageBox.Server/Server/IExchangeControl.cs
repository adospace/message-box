namespace MessageBox.Server;

public interface IExchangeControl
{
    string Key { get; }

    int GetTotalMessageCount();

    int GetCurrentMessageCount();

    IReadOnlyList<IQueueControl> GetSubscribers();

    bool IsAlive(TimeSpan keepAliveTimeout);
}