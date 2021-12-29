namespace MessageBox
{
    public interface ITransport
    {
        Task Run(Func<CancellationToken, Task>? onConnectionSucceed = null, Func<CancellationToken, Task>? onConnectionEnded = null, CancellationToken cancellationToken = default);

        Task Stop(CancellationToken cancellationToken = default);
    }
}