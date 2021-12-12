namespace MessageBox.Tcp
{
    public abstract class TcpTransport : ITransport
    {
        public abstract Task Run(CancellationToken cancellationToken = default);

        public abstract Task Stop(CancellationToken cancellationToken = default);
    }
}