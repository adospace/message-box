namespace MessageBox.Server.Tcp.Host.Client.Services;

public static class ServiceCollectionExtensions
{
    public static void AddServerServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IMessageStatisticService, Implementation.MessageStatisticService>();
    }

}