using MessageBox;
using Microsoft.Extensions.Hosting;
using SharedModels;

using var clientHost = Host.CreateDefaultBuilder()
    .AddMessageBoxTcpClient(System.Net.IPAddress.Loopback, 12000)
    .AddConsumer<SampleConsumer>()
    .AddJsonSerializer()
    .Build();

clientHost.Run();


class SampleConsumer : IHandler<EventModel>
{
    public Task Handle(IMessageContext<EventModel> messageContext, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Received event from client: {messageContext.Model.Description}");
        return Task.CompletedTask;
    }
}