using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Testing.Implementation
{
    internal class ServerTransport : ITransport
    {
        
        public ServerTransport(IMessageSource source, IMessageSink sink)
        {
            Source = source;
            Sink = sink;
        }

        public IMessageSource Source { get; }

        public IMessageSink Sink { get; }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
