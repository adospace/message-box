namespace MessageBox.Server;

public interface IBusServerControl
{
    IReadOnlyList<IQueueControl> GetQueues();

    IReadOnlyList<IExchangeControl> GetExchanges();

    void DeleteQueue(Guid id);

    void DeleteExchange(string name);
}