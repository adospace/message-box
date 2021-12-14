using MessageBox.Client;
using MessageBox.Serializer.Json.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

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
            serviceCollection.AddTransient(sp => new JsonSerializerConfigurator(optionsConfigureAction));

            return serviceCollection;
        }

        public static IHostBuilder AddJsonSerializer(this IHostBuilder hostBuilder)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddJsonSerializer());

            return hostBuilder;
        }

        public static IHostBuilder ConfigureJsonSerializer(this IHostBuilder hostBuilder, Action<JsonSerializerOptions> optionsConfigureAction)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.ConfigureJsonSerializer(optionsConfigureAction));

            return hostBuilder;
        }
    }
}