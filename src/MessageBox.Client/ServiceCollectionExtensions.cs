using MessageBox.Client.Implementation;
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
        public static IServiceCollection AddMessageBoxClient(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<Bus>();
            serviceCollection.AddSingleton<IBus>(sp => sp.GetRequiredService<Bus>());
            serviceCollection.AddSingleton<IMessageSink>(sp => sp.GetRequiredService<Bus>());
            serviceCollection.AddSingleton<IMessageSource>(sp => sp.GetRequiredService<Bus>());
            serviceCollection.AddSingleton<IBusClient>(sp => sp.GetRequiredService<Bus>());

            return serviceCollection;
        }

        //public static IServiceCollection AddHandler<T, S>(this IServiceCollection serviceCollection) where S : IHandler<T>
        //{
        //    serviceCollection.AddSingleton<IMessageReceiverCallback>(sp => new MessageReceiverCallbackWithoutReturnValue(
        //        typeof(T), async (message, model, cancellationToken) =>
        //    {
        //        var handler = sp.GetRequiredService<S>();
        //        try
        //        {
        //            var messageContext = new MessageContext<T>((T)model, message);
        //            await handler.Handle(messageContext, cancellationToken);
        //        }
        //        catch (Exception)
        //        {
        //            throw;
        //        }
        //    }));

        //    return serviceCollection;
        //}
        //public static IServiceCollection AddHandler<T, RType, S>(this IServiceCollection serviceCollection) where S : IHandler<T, RType>
        //{
        //    serviceCollection.AddSingleton<IMessageReceiverCallback>(sp => new MessageReceiverCallbackWithReturnValue(
        //        typeof(T), async (message, model, cancellationToken) =>
        //        {
        //            var handler = sp.GetRequiredService<S>();
        //            try
        //            {
        //                var messageContext = new MessageContext<T>((T)model, message);
        //                return await handler.Handle(messageContext, cancellationToken);
        //            }
        //            catch (Exception)
        //            {
        //                throw;
        //            }
        //        }));

        //    return serviceCollection;
        //}

        private readonly static Type _messageContextType = typeof(MessageContext<>);
        private readonly static Type _taskType = typeof(Task<>);

        public static IServiceCollection AddConsumer<T>(this IServiceCollection serviceCollection) where T : class
        {
            serviceCollection.AddScoped<T>();

            var typeOfIHandler = typeof(IHandler);
            var listOfHandlerTypes = typeof(T).GetInterfaces().Where(_ => _ != typeOfIHandler && typeOfIHandler.IsAssignableFrom(_)).ToArray();

            foreach (var handlerType in listOfHandlerTypes)
            {
                var typeOfModel = handlerType.GenericTypeArguments[0];

                var messageContextActualType = _messageContextType.MakeGenericType(new[] { typeOfModel });
                var handleMethod = handlerType.GetMethod("Handle") ?? throw new InvalidOperationException();
                if (handlerType.GenericTypeArguments.Length == 1)
                {
                    serviceCollection.AddSingleton<IMessageReceiverCallback>(sp => new MessageReceiverCallbackWithoutReturnValue(
                        typeOfModel, async (message, model, cancellationToken) =>
                    {
                        var handler = sp.GetRequiredService<T>();
                        try
                        {
                            var messageContext = Activator.CreateInstance(messageContextActualType, model, message);
                            await (Task)(handleMethod.Invoke(handler, new[] { messageContext, cancellationToken }) ?? throw new InvalidOperationException());
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }));
                }
                else
                {
                    var typeOfReplyModel = handlerType.GenericTypeArguments[1];
                    var returnTaskType = _taskType.MakeGenericType(new[] { typeOfReplyModel });
                    var resultProperty = returnTaskType.GetProperty("Result") ?? throw new InvalidOperationException();

                    serviceCollection.AddSingleton<IMessageReceiverCallback>(sp => new MessageReceiverCallbackWithReturnValue(
                        typeOfModel, async (message, model, cancellationToken) =>
                        {
                            var handler = sp.GetRequiredService<T>();
                            try
                            {
                                var messageContext = Activator.CreateInstance(messageContextActualType, model, message);
                                var task = (Task)(handleMethod.Invoke(handler, new[] { messageContext, cancellationToken }) ?? throw new InvalidOperationException());
                                await task;

                                return resultProperty.GetValue(task);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                        }));
                }
            }

            return serviceCollection;
        }
    }
}
