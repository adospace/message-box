using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxBackgroundService(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddHostedService<Implementation.BusService>();

            return serviceCollection;
        }

        public static IHostBuilder AddMessageBoxBackgroundService(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddMessageBoxBackgroundService());
            return hostBuilder;
        }
    }

}
