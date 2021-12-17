using MessageBox;
using Microsoft.Extensions.Hosting;
using SharedModels;

using var clientHost = Host.CreateDefaultBuilder()
    .AddMessageBoxTcpClient(System.Net.IPAddress.Loopback, 12000)
    .AddConsumer<SampleConsumer>()
    .AddJsonSerializer()
    .Build();

clientHost.Run();


class SampleConsumer : IHandler<EventModel>, IHandler<ExecuteCommandModel>, IHandler<ExecuteCommandWithReplyModel, CommandResultModel>
{
    public Task Handle(IMessageContext<EventModel> messageContext, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Received event from client: {messageContext.Model.Description}");
        return Task.CompletedTask;
    }

    public Task Handle(IMessageContext<ExecuteCommandModel> messageContext, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Executing command with parameters: {messageContext.Model}");
        return Task.CompletedTask;
    }

    public Task<CommandResultModel> Handle(IMessageContext<ExecuteCommandWithReplyModel> messageContext, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Executing command and reply with parameters: {messageContext.Model}");
        return Task.FromResult(new CommandResultModel(messageContext.Model.X  * 2));
    }
}