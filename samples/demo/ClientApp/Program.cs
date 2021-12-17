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

string? valueString;
while ((valueString = Console.ReadLine()) != null)
{
    if (int.TryParse(valueString, out var value))
    {
        await client.Send(new ExecuteCommandModel(value));

        var reply = await client.SendAndGetReply<CommandResultModel>(new ExecuteCommandWithReplyModel(value));
        Console.WriteLine($"Reply from consumer: {reply}");
    }
}

