namespace MessageBox.Server;

public interface IExchangeControl
{
    string Key { get; }

    int GetMessageCount();

    IReadOnlyList<IQueueControl> GetSubscribers();
}