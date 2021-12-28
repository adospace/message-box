using AdminDemoClient;
using MessageBox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


using var clientHost = Host.CreateDefaultBuilder()
    .AddMessageBoxTcpClient(System.Net.IPAddress.Loopback, 12000)
    .AddJsonSerializer()
    .AddConsumer<DemoConsumer>()
    .Build();

clientHost.Start();

IBusClient client = clientHost.Services.GetRequiredService<IBusClient>();

CancellationTokenSource cancellationTokenSource = new();
Console.CancelKeyPress += (sender, e) =>
{
    Console.WriteLine("Exiting...");
    cancellationTokenSource.Cancel();
};

try
{
    while (!cancellationTokenSource.IsCancellationRequested)
    {
        // do something
        await Task.Delay(1000, cancellationTokenSource.Token);
        
        Console.WriteLine("Sending call...");

        await client.Send(new DemoModel("parameter-value"), cancellationToken: cancellationTokenSource.Token);

        Console.WriteLine("Publishing event...");
        await client.Publish(new DemoEventModel(123), cancellationToken: cancellationTokenSource.Token);
        
        Console.WriteLine("Done");
    }
}
catch (OperationCanceledException)
{
    
}
