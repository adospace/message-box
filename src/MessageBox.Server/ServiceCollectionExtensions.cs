using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Server
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxServer(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IBus, Implementation.Bus>();
            serviceCollection.AddSingleton<IMessageSink>(sp => (Implementation.Bus)sp.GetRequiredService<IBus>());
            serviceCollection.AddSingleton<IBusServer>(sp => (Implementation.Bus)sp.GetRequiredService<IBus>());

            return serviceCollection;
        }
    }
}
