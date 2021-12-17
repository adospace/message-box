using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
