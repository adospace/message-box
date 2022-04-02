namespace MessageBox.Server;

public interface IQueueControl
{
    Guid Id { get; }

    string Name { get; }

    int GetTotalMessageCount();
    
    int GetCurrentMessageCount();

    Task TryToKeepAlive(CancellationToken cancellationToken = default);
    
}