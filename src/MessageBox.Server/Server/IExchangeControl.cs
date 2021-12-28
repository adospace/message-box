namespace MessageBox.Server;

public interface IExchangeControl
{
    string Key { get; }

    int GetTotalMessageCount();

    IReadOnlyList<IQueueControl> GetSubscribers();

    bool IsAlive(TimeSpan keepAliveTimeout);
}