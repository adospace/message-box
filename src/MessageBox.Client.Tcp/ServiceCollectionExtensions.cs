using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Client.Tcp
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxTcpClient(this IServiceCollection serviceCollection, TcpTransportOptions options)
        {
            serviceCollection.AddMessageBoxClient();
            serviceCollection.AddSingleton<ITransportFactory>(sp => new Client.Tcp.Implementation.ClientTransportFactory(sp, options));
            serviceCollection.AddMessageBoxBackgroundService();

            return serviceCollection;
        }

        public static IHostBuilder AddMessageBoxTcpClient(this IHostBuilder hostBuilder, TcpTransportOptions options)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddMessageBoxTcpClient(options));
            return hostBuilder;
        }

        public static IHostBuilder AddMessageBoxTcpClient(this IHostBuilder hostBuilder, int port)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddMessageBoxTcpClient(new TcpTransportOptions(port)));
            return hostBuilder;
        }

        public static IHostBuilder AddMessageBoxTcpClient(this IHostBuilder hostBuilder, string ipString, int port)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddMessageBoxTcpClient(new TcpTransportOptions(ipString, port)));
            return hostBuilder;
        }
    }

}
