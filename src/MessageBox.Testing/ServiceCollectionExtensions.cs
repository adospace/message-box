using MessageBox.Client;
using MessageBox.Server;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageBox.Testing
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxInMemoryServer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddMessageBoxServer();
            serviceCollection.AddSingleton<ITransportFactory, Implementation.ServerTransportFactory>();
            serviceCollection.AddHostedService<Implementation.BusService>();

            return serviceCollection;
        }
        public static IServiceCollection AddMessageBoxInMemoryClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddMessageBoxClient();
            serviceCollection.AddSingleton<ITransportFactory, Implementation.ClientTransportFactory>();
            serviceCollection.AddHostedService<Implementation.BusService>();

            return serviceCollection;
        }

        public static IHostBuilder AddMessageBoxInMemoryServer(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddMessageBoxInMemoryServer());
            return hostBuilder;
        }

        public static IHostBuilder AddMessageBoxInMemoryClient(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddMessageBoxInMemoryClient());
            return hostBuilder;
        }
    }
}
