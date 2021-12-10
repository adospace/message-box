using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Client
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IBus, Implementation.Bus>();
            serviceCollection.AddSingleton<IMessageSink>(sp => (Implementation.Bus)sp.GetRequiredService<IBus>());
            serviceCollection.AddSingleton<IBusClient>(sp => (Implementation.Bus)sp.GetRequiredService<IBus>());

            return serviceCollection;
        }
    }
}
