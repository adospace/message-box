using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace MessageBox
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxTcpClient(this IServiceCollection serviceCollection, TcpBusClientOptions options)
        {
            serviceCollection.AddMessageBoxClient(options);
            serviceCollection.AddSingleton<ITransportFactory>(sp => new Client.Implementation.ClientTransportFactory(sp, options));
            serviceCollection.AddMessageBoxBackgroundService();

            return serviceCollection;
        }

        public static IHostBuilder AddMessageBoxTcpClient(this IHostBuilder hostBuilder, TcpBusClientOptions options)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddMessageBoxTcpClient(options));
            return hostBuilder;
        }

        public static IHostBuilder AddMessageBoxTcpClient(this IHostBuilder hostBuilder, IPAddress address, int port)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddMessageBoxTcpClient(new TcpBusClientOptions(address, port)));
            return hostBuilder;
        }

        public static IHostBuilder AddMessageBoxTcpClient(this IHostBuilder hostBuilder, string ipString, int port)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddMessageBoxTcpClient(new TcpBusClientOptions(ipString, port)));
            return hostBuilder;
        }
        public static IHostBuilder AddMessageBoxTcpClient(this IHostBuilder hostBuilder, Func<TcpBusClientOptions> configureBusAction)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddMessageBoxTcpClient(configureBusAction()));
            return hostBuilder;
        }
    }
}
