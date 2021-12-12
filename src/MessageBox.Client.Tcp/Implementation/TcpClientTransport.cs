using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Client.Tcp.Implementation
{
    internal class TcpClientTransport : ITransport
    {
        private readonly IServiceProvider _sp;
        private readonly TcpTransportOptions _options;

        public TcpClientTransport(IServiceProvider sp, TcpTransportOptions options)
        {
            _sp = sp;
            _options = options;
        }

        public Task Run(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task Stop(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
