using MessageBox.Server;
using MessageBox.Server.Implementation;
using Microsoft.Extensions.DependencyInjection;

namespace MessageBox
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxServer(this IServiceCollection serviceCollection, ICleanUpServiceOptions? options = null)
        {
            serviceCollection.AddSingleton<Server.Implementation.Bus>();
            serviceCollection.AddSingleton<IBus>(sp => sp.GetRequiredService<Server.Implementation.Bus>());
            serviceCollection.AddSingleton<IMessageSink>(sp => sp.GetRequiredService<Server.Implementation.Bus>());
            serviceCollection.AddSingleton<Server.IBusServer>(sp => sp.GetRequiredService<Server.Implementation.Bus>());
            serviceCollection.AddSingleton<IBusServerControl>(sp => sp.GetRequiredService<Server.Implementation.Bus>());
            serviceCollection.AddHostedService<CleanUpService>(sp =>
                new CleanUpService(sp.GetRequiredService<IBusServerControl>(), options));

            return serviceCollection;
        }
    }
}
