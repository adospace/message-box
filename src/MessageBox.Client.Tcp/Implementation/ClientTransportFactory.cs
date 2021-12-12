using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Client.Tcp.Implementation
{
    internal class ClientTransportFactory : ITransportFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TcpTransportOptions _options;

        public ClientTransportFactory(IServiceProvider serviceProvider, TcpTransportOptions options)
        {
            _serviceProvider = serviceProvider;
            _options = options;
        }

        public ITransport Create()
        {
            return new TcpClientTransport(_serviceProvider, _options);
        }
    }
}
