using MessageBox.Tcp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Server.Tcp.Implementation
{
    internal class TcpServerTransport : TcpTransport
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TcpTransportOptions _options;

        public TcpServerTransport(IServiceProvider serviceProvider, TcpTransportOptions options)
        {
            _serviceProvider = serviceProvider;
            _options = options;
        }

        public override Task Run(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public override Task Stop(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
