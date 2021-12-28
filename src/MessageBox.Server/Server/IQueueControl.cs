namespace MessageBox.Server;

public interface IQueueControl
{
    Guid Id { get; }

    string? Name { get; }

    int GetMessageCount();

    Task<bool> IsAlive(TimeSpan keepAliveTimeout, CancellationToken cancellationToken = default);
}