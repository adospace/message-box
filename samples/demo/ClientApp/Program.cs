using MessageBox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharedModels;

using var clientHost = Host.CreateDefaultBuilder()
    .AddMessageBoxTcpClient(System.Net.IPAddress.Loopback, 12000)
    .AddJsonSerializer()
    .Build();

clientHost.Start();

IBusClient client = clientHost.Services.GetRequiredService<IBusClient>();

string? eventDesc;
while ((eventDesc = Console.ReadLine()) != null)
{
    await client.Publish(new EventModel(eventDesc));
}

