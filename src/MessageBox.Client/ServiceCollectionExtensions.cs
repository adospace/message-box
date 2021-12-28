using MessageBox.Client.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MessageBox
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageBoxClient(this IServiceCollection serviceCollection, IBusClientOptions options)
        {
            serviceCollection.AddSingleton(sp => new Bus(sp, options));
            serviceCollection.AddSingleton<IBus>(sp => sp.GetRequiredService<Bus>());
            serviceCollection.AddSingleton<IMessageSink>(sp => sp.GetRequiredService<Bus>());
            serviceCollection.AddSingleton<IMessageSource>(sp => sp.GetRequiredService<Bus>());
            serviceCollection.AddSingleton<IBusClient>(sp => sp.GetRequiredService<Bus>());

            return serviceCollection;
        }

        private static readonly Type MessageContextType = typeof(MessageContext<>);
        private static readonly Type TaskType = typeof(Task<>);

        public static IServiceCollection AddConsumer<T>(this IServiceCollection serviceCollection, ServiceLifetime lifetime = ServiceLifetime.Scoped) where T : class
        {
            serviceCollection.Add(new ServiceDescriptor(typeof(T), typeof(T), lifetime));

            AddHandlersOfConsumer<T>(serviceCollection);

            return serviceCollection;
        }
        public static IServiceCollection AddConsumer<T>(this IServiceCollection serviceCollection, T instance) where T : class
        {
            serviceCollection.Add(new ServiceDescriptor(typeof(T), instance));

            AddHandlersOfConsumer(serviceCollection, instance);

            return serviceCollection;
        }

        private static void AddHandlersOfConsumer<T>(this IServiceCollection serviceCollection, T? instance = null) where T : class
        { 
            var typeOfIHandler = typeof(IHandler);
            var listOfHandlerTypes = typeof(T).GetInterfaces().Where(_ => _ != typeOfIHandler && typeOfIHandler.IsAssignableFrom(_)).ToArray();

            foreach (var handlerType in listOfHandlerTypes)
            {
                var typeOfModel = handlerType.GenericTypeArguments[0];

                var messageContextActualType = MessageContextType.MakeGenericType(new[] { typeOfModel });
                var handleMethod = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException();
                if (handlerType.GenericTypeArguments.Length == 1)
                {
                    serviceCollection.AddSingleton<IMessageReceiverCallback>(sp => new MessageReceiverCallbackWithoutReturnValue(
                        typeOfModel, async (_, model, cancellationToken) =>
                    {
                        using var scope = sp.CreateScope();
                        var handler = instance ?? scope.ServiceProvider.GetRequiredService<T>();
                        var messageContext = Activator.CreateInstance(messageContextActualType, model);
                        await (Task)(handleMethod.Invoke(handler, new[] { messageContext, cancellationToken }) ?? throw new InvalidOperationException());
                    }));
                }
                else
                {
                    var typeOfReplyModel = handlerType.GenericTypeArguments[1];
                    var returnTaskType = TaskType.MakeGenericType(new[] { typeOfReplyModel });
                    var resultProperty = returnTaskType.GetProperty("Result") ?? throw new InvalidOperationException();

                    serviceCollection.AddSingleton<IMessageReceiverCallback>(sp => new MessageReceiverCallbackWithReturnValue(
                        typeOfModel, async (_, model, cancellationToken) =>
                        {
                            using var scope = sp.CreateScope();
                            var handler = instance ?? scope.ServiceProvider.GetRequiredService<T>();
                            var messageContext = Activator.CreateInstance(messageContextActualType, model);
                            var task = (Task)(handleMethod.Invoke(handler, new[] { messageContext, cancellationToken }) ?? throw new InvalidOperationException());
                            await task;

                            return resultProperty.GetValue(task);
                        }));
                }
            }
        
        }

        public static IHostBuilder AddConsumer<T>(this IHostBuilder hostBuilder, ServiceLifetime lifetime = ServiceLifetime.Scoped) where T : class
        {
            hostBuilder.ConfigureServices((_, services) => services.AddConsumer<T>(lifetime));

            return hostBuilder;
        }
        public static IHostBuilder AddConsumer<T>(this IHostBuilder hostBuilder, T instance) where T : class
        {
            hostBuilder.ConfigureServices((_, services) => services.AddConsumer(instance));

            return hostBuilder;
        }
    }
}
