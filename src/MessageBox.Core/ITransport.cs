namespace MessageBox
{
    public interface ITransport
    {
        Task Run(CancellationToken cancellationToken = default);

        Task Stop(CancellationToken cancellationToken = default);
    }
}