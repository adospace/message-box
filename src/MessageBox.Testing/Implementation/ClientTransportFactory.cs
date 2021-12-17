namespace MessageBox.Testing.Implementation
{
    internal class ClientTransportFactory : ITransportFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ClientTransportFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ITransport Create()
        {
            return new ClientTransport(_serviceProvider);
        }
    }
}
