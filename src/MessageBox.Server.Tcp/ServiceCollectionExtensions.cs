using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageBox
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxTcpServer(this IServiceCollection serviceCollection, TcpTransportOptions options)
        {
            serviceCollection.AddMessageBoxServer();
            serviceCollection.AddSingleton<ITransportFactory>(sp => new Server.Tcp.Implementation.ServerTransportFactory(sp, options));
            serviceCollection.AddMessageBoxBackgroundService();

            return serviceCollection;
        }

        public static IHostBuilder AddMessageBoxTcpServer(this IHostBuilder hostBuilder, TcpTransportOptions options)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddMessageBoxTcpServer(options));
            return hostBuilder;
        }

        public static IHostBuilder AddMessageBoxTcpServer(this IHostBuilder hostBuilder, int port)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddMessageBoxTcpServer(new TcpTransportOptions(port)));
            return hostBuilder;
        }
        
        public static IHostBuilder AddMessageBoxTcpServer(this IHostBuilder hostBuilder, string ipString, int port)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddMessageBoxTcpServer(new TcpTransportOptions(ipString, port)));
            return hostBuilder;
        }
    }
}