using MessageBox;

namespace AdminDemoClient;

public class DemoConsumer : IHandler<DemoModel>, IHandler<DemoEventModel>
{
    public Task Handle(IMessageContext<DemoModel> messageContext, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Received call {messageContext.Model}");
        return  Task.CompletedTask;
    }

    public Task Handle(IMessageContext<DemoEventModel> messageContext, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Received event {messageContext.Model}");
        return Task.CompletedTask;
    }
}