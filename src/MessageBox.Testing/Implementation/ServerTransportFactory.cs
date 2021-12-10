using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Testing.Implementation
{
    internal class ServerTransportFactory : ITransportFactory
    {
        public ITransport Create(IMessageSource source, IMessageSink sink)
        {
            return new ServerTransport(source, sink);
        }
    }
}
