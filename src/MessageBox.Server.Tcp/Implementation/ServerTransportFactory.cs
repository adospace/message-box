using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Server.Tcp.Implementation
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
