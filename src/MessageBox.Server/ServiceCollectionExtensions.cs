using MessageBox.Server;
using MessageBox.Server.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MessageBox
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxServer(this IServiceCollection serviceCollection, ICleanUpServiceOptions? options = null)
        {
            serviceCollection.AddSingleton<Bus>();
            serviceCollection.AddSingleton<IBus>(sp => sp.GetRequiredService<Bus>());
            serviceCollection.AddSingleton<IMessageSink>(sp => sp.GetRequiredService<Bus>());
            serviceCollection.AddSingleton<IBusServer>(sp => sp.GetRequiredService<Bus>());
            serviceCollection.AddSingleton<IBusServerControl>(sp => sp.GetRequiredService<Bus>());
            serviceCollection.AddHostedService(sp =>
                new CleanUpService(sp.GetRequiredService<IBusServerControl>(), sp.GetRequiredService<ILogger<CleanUpService>>(), options));

            return serviceCollection;
        }
    }
}
