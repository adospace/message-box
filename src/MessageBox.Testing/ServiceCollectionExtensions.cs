using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Testing
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryMessageBox(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<ITransportFactory, Implementation.ClientTransportFactory>();

            return serviceCollection;
        }
    }
}
