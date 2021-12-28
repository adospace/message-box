namespace MessageBox.Server;

public interface IBusServerControl
{
    IReadOnlyList<IQueueControl> GetQueues();

    IReadOnlyList<IExchangeControl> GetExchanges();
}