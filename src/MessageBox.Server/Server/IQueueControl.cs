namespace MessageBox.Server;

public interface IQueueControl
{
    Guid Id { get; }

    string? Name { get; }

    int GetTotalMessageCount();

    Task<bool> IsAlive(TimeSpan keepAliveTimeout, CancellationToken cancellationToken = default);
}