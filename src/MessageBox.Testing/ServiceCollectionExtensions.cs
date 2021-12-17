using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// ReSharper disable MemberCanBePrivate.Global

namespace MessageBox.Testing
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxInMemoryServer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddMessageBoxServer();
            serviceCollection.AddSingleton<ITransportFactory, Implementation.ServerTransportFactory>();
            serviceCollection.AddMessageBoxBackgroundService();

            return serviceCollection;
        }
        public static IServiceCollection AddMessageBoxInMemoryClient(this IServiceCollection serviceCollection, InMemoryBusClientOptions? options = null)
        {
            serviceCollection.AddMessageBoxClient(options ?? new InMemoryBusClientOptions());
            serviceCollection.AddSingleton<ITransportFactory, Implementation.ClientTransportFactory>();
            serviceCollection.AddMessageBoxBackgroundService();

            return serviceCollection;
        }

        public static IHostBuilder AddMessageBoxInMemoryServer(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((_, services) => services.AddMessageBoxInMemoryServer());
            return hostBuilder;
        }

        public static IHostBuilder AddMessageBoxInMemoryClient(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((_, services) => services.AddMessageBoxInMemoryClient());
            return hostBuilder;
        }
    }
}
