namespace MessageBox
{
    public interface IBusClient
    {
        Task Publish<T>(T model, CancellationToken cancellationToken = default);

        Task Send<T>(T model, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        Task<R> SendAndGetReply<R>(object model, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    }
}
