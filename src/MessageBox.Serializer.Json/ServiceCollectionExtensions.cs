using MessageBox.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace MessageBox
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJsonSerializer(this IServiceCollection serviceCollection, JsonSerializerOptions? options = null)
        {
            serviceCollection.AddSingleton<IMessageSerializerFactory>(sp => new Serializer.Json.Implementation.JsonMessageSerializerFactory(options));

            return serviceCollection;        
        }

        public static IHostBuilder AddJsonSerializer(this IHostBuilder hostBuilder, JsonSerializerOptions? options = null)
        {
            hostBuilder.ConfigureServices((ctx, services) => services.AddJsonSerializer(options));

            return hostBuilder;
        }
    }
}