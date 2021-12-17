using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// ReSharper disable MemberCanBePrivate.Global

namespace MessageBox
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxTcpServer(this IServiceCollection serviceCollection, TcpTransportOptions options)
        {
            serviceCollection.AddMessageBoxServer();
            serviceCollection.AddSingleton<ITransportFactory>(sp => new Server.Implementation.ServerTransportFactory(sp, options));
            serviceCollection.AddMessageBoxBackgroundService();

            return serviceCollection;
        }

        public static IHostBuilder AddMessageBoxTcpServer(this IHostBuilder hostBuilder, TcpTransportOptions options)
        {
            hostBuilder.ConfigureServices((_, services) => services.AddMessageBoxTcpServer(options));
            return hostBuilder;
        }

        public static IHostBuilder AddMessageBoxTcpServer(this IHostBuilder hostBuilder, int port)
        {
            hostBuilder.ConfigureServices((_, services) => services.AddMessageBoxTcpServer(new TcpTransportOptions(port)));
            return hostBuilder;
        }
        
        public static IHostBuilder AddMessageBoxTcpServer(this IHostBuilder hostBuilder, string ipString, int port)
        {
            hostBuilder.ConfigureServices((_, services) => services.AddMessageBoxTcpServer(new TcpTransportOptions(ipString, port)));
            return hostBuilder;
        }
    }
}