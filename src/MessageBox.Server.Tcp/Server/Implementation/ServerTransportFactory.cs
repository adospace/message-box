namespace MessageBox.Server.Implementation
{
    internal class ServerTransportFactory : ITransportFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TcpTransportOptions _options;

        public ServerTransportFactory(IServiceProvider serviceProvider, TcpTransportOptions options)
        {
            _serviceProvider = serviceProvider;
            _options = options;
        }

        public ITransport Create()
        {
            return new TcpServerTransport(_serviceProvider, _options);
        }
    }
}
