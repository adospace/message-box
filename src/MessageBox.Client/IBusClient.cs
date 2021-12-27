namespace MessageBox
{
    public interface IBusClient
    {
        Task Publish<T>(T model, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        Task Send<T>(T model, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

        Task<TR> SendAndGetReply<TR>(object model, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
    }
}
