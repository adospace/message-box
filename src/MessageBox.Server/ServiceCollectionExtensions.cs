using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxServer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<Server.Implementation.Bus>();
            serviceCollection.AddSingleton<IBus>(sp => sp.GetRequiredService<Server.Implementation.Bus>());
            serviceCollection.AddSingleton<IMessageSink>(sp => sp.GetRequiredService<Server.Implementation.Bus>());
            serviceCollection.AddSingleton<Server.IBusServer>(sp => sp.GetRequiredService<Server.Implementation.Bus>());

            return serviceCollection;
        }
    }
}
