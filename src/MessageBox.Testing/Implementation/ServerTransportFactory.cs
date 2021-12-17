namespace MessageBox.Testing.Implementation
{
    internal class ServerTransportFactory : ITransportFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServerTransportFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ITransport Create()
        {
            return new ServerTransport(_serviceProvider);
        }
    }
}
