using MessageBox.Server;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Testing
{
    public class InMemoryServer : IDisposable
    {
        private bool _disposed;
        private IServiceProvider? _serviceProvider;

        public InMemoryServer()
        {

        }

        public void Start()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddMessageBoxServer();
            serviceCollection.AddSingleton<ITransportFactory, Implementation.ServerTransportFactory>();

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public void Stop()
        { 
        
        }

        public void Dispose()
        {
            if (_disposed)
            { 
                throw new ObjectDisposedException(nameof(InMemoryServer));
            }

            Stop();

            _disposed = true;
        }
    }
}
