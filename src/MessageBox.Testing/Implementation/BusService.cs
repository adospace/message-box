using MessageBox.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Testing.Implementation
{
    internal class BusService : BackgroundService
    {
        private readonly IBus _bus;

        public BusService(IBus bus)
        {
            _bus = bus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _bus.Run(stoppingToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return _bus.Stop(cancellationToken);
        }
    }
}
