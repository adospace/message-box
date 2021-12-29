using AdminDemoClient;
using MessageBox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


using var clientHost = Host.CreateDefaultBuilder()
    .AddMessageBoxTcpClient(new TcpBusClientOptions(System.Net.IPAddress.Loopback, 12000){ DefaultCallTimeout = TimeSpan.FromSeconds(2)})
    .AddJsonSerializer()
    .AddConsumer<DemoConsumer>()
    .Build();

clientHost.Start();

var client = clientHost.Services.GetRequiredService<IBusClient>();

CancellationTokenSource cancellationTokenSource = new();
Console.CancelKeyPress += (_, _) =>
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
        
        try
        {
            Console.WriteLine("Sending call...");
            
            await client.Send(new DemoModel("parameter-value"), cancellationToken: cancellationTokenSource.Token);

            Console.WriteLine("Publishing event...");
            await client.Publish(new DemoEventModel(123), cancellationToken: cancellationTokenSource.Token);
        
            Console.WriteLine("Done");
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Timeout!");
        }
    }
}
catch (OperationCanceledException)
{
    
}
