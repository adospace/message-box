using MessageBox.Client;
using MessageBox.Serializer.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
// ReSharper disable MemberCanBePrivate.Global

namespace MessageBox
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJsonSerializer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IMessageSerializerFactory, JsonMessageSerializerFactory>();

            return serviceCollection;        
        }

        public static IServiceCollection ConfigureJsonSerializer(this IServiceCollection serviceCollection, Action<JsonSerializerOptions> optionsConfigureAction)
        {
            serviceCollection.AddTransient(_ => new JsonSerializerConfigurator(optionsConfigureAction));

            return serviceCollection;
        }

        public static IHostBuilder AddJsonSerializer(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((_, services) => services.AddJsonSerializer());

            return hostBuilder;
        }

        public static IHostBuilder ConfigureJsonSerializer(this IHostBuilder hostBuilder, Action<JsonSerializerOptions> optionsConfigureAction)
        {
            hostBuilder.ConfigureServices((_, services) => services.ConfigureJsonSerializer(optionsConfigureAction));

            return hostBuilder;
        }
    }
}