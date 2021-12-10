using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Testing.Implementation
{
    internal class ClientTransport : ITransport
    {

        private CancellationTokenSource _cts = new CancellationTokenSource();

        public ClientTransport(IMessageSource source, IMessageSink sink)
        { 
            Source = source;
            Sink = sink;
        }
        public IMessageSource Source { get; }

        public IMessageSink Sink { get; }

        public async void Start()
        {
            while (!_cts.IsCancellationRequested)
            {
                var messageToSend = await Source.GetNextMessageToSend(_cts.Token);            

                
            }
        }

        public void Stop()
        {
            
        }
    }
}
