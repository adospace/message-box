using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// ReSharper disable MemberCanBePrivate.Global

namespace MessageBox
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxTcpServer(this IServiceCollection serviceCollection, ServerTcpTransportOptions options)
        {
            serviceCollection.AddMessageBoxServer();
            serviceCollection.AddSingleton<ITransportFactory>(sp => new Server.Implementation.ServerTransportFactory(sp, options));
            serviceCollection.AddMessageBoxBackgroundService();

            return serviceCollection;
        }

        public static IServiceCollection AddMessageBoxTcpServer(this IServiceCollection serviceCollection, int port)
        {
            return AddMessageBoxTcpServer(serviceCollection, new ServerTcpTransportOptions(port));
        }

        public static IServiceCollection AddMessageBoxTcpServer(this IServiceCollection serviceCollection, string ipString, int port)
        {
            return AddMessageBoxTcpServer(serviceCollection, new ServerTcpTransportOptions(ipString, port));
        }

        public static IHostBuilder AddMessageBoxTcpServer(this IHostBuilder hostBuilder, ServerTcpTransportOptions options)
        {
            hostBuilder.ConfigureServices((_, services) => services.AddMessageBoxTcpServer(options));
            return hostBuilder;
        }

        public static IHostBuilder AddMessageBoxTcpServer(this IHostBuilder hostBuilder, int port)
        {
            hostBuilder.ConfigureServices((_, services) => services.AddMessageBoxTcpServer(new ServerTcpTransportOptions(port)));
            return hostBuilder;
        }
        
        public static IHostBuilder AddMessageBoxTcpServer(this IHostBuilder hostBuilder, string ipString, int port)
        {
            hostBuilder.ConfigureServices((_, services) => services.AddMessageBoxTcpServer(new ServerTcpTransportOptions(ipString, port)));
            return hostBuilder;
        }
    }
}