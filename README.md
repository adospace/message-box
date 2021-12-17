# MessageBox .NET Message Broker
MessageBox is a .NET 6 message broker that aims to simplify message communication between services in a microservice architecture. 

It's entirely written in .NET/C# and tries to use all the best practices and latest features of the framework in order to reduce memory consumption and CPU usage while leverage optimal perfomance for many use cases. 

It's not a full fledged product nor it aims to be a one tool for every scenario instead it tries to make message interprocess communication easy to develop and test.

[![Build status](https://ci.appveyor.com/api/projects/status/dkyae4p5jagnu3k4?svg=true)](https://ci.appveyor.com/project/adospace/message-box)
[![codecov](https://codecov.io/gh/adospace/message-box/branch/main/graph/badge.svg?token=3M6R96NL54)](https://codecov.io/gh/adospace/message-box)

Main features
- ## Native implementation in .NET highly optimized to reduce memory occupation and CPU usage
  Socket communication is realized with Pipelines (https://docs.microsoft.com/en-us/dotnet/standard/io/pipelines)
  
  Producer/Consumer patterns implementated in TPL (https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl)
  
  Memory pooling (https://docs.microsoft.com/en-us/dotnet/api/system.buffers.memorypool-1?view=net-6.0)
  
- ## Standard .NET framework integrations
  MessageBox is natively delivered as MS Dependency Injection services fully compatible with the hosting pattern https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-6.0
  Supports for pluggable message serializer like JSON, BSON, MessagePack etc
  
  Logging is structured and delivered using the standard MS Logging extensions (https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-6.0)
  
  Provides out of the box testing enviroment that let developer unit test services and consumers with in-memory message communication
  
  100% Thread-safe, fully async-await implementation with cancellation
  
  Ability to configure the level of parallelism used to call consumers handlers

- ## Deployment
  A fully working pre-built broker server is provided in the release or as docker container (it also includes a pretty admin dashboard)
  
  You can also integrate exactly the same brocker service in a ASP.NET core project
  
- ## License
  MessageBox is and always will be completely open-source under the permissive MIT license
  

### Getting started
In the following sample it's shown how to implement the 2 main message-oriented patterns:

1. Publish-Subscribe pattern (https://en.wikipedia.org/wiki/Publish%E2%80%93subscribe_pattern)
2. RPC pattern (https://en.wikipedia.org/wiki/Remote_procedure_call)

Let's start creating 4 minimal .NET 6 projects (you can use VS, VS Code, Rider, dotnet cli etc).

The resulting folder structure should be similar to:
```
+---ClientApp
|   |   ClientApp.csproj
|   |   Program.cs
+---ConsumerApp
|   |   ConsumerApp.csproj
|   |   Program.cs
+---ServerApp
|   |   Program.cs
|   |   ServerApp.csproj
\---SharedModels
    |   EventModel.cs
    |   SharedModels.csproj
```

Now create a model of the message that we want to publish from a client to a consumer. Open the EventModel.cs file under SharedModels project and add a sample record like this:
```c#
namespace SharedModels
{
    public record EventModel(string Description);
}
```

Add a reference to the SharedModels project inside the Client and Consumer projects.

Now open the server app project and the references to the MessageBox.Tcp.Server package:
```
<ItemGroup>
  <PackageReference Include="MessageBox.Tcp.Server" Version="1.0.3" />
  <PackageReference Include="Microsoft.Extensions.Hosting" Version="6.0.0" />
</ItemGroup>
```

Client and consumer projects should reference the MessageBox.Tcp.Client package instead and MessageBox.Serializer.Json:
```
<ItemGroup>
  <PackageReference Include="MessageBox.Tcp.Client" Version="1.0.3" />
  <PackageReference Include="MessageBox.Serializer.Json" Version="1.0.3" />
  <PackageReference Include="Microsoft.Extensions.Hosting" Version="6.0.0" />
</ItemGroup>
```

Add the following code to the server program.cs
```c#
using var serverHost = Host.CreateDefaultBuilder()
    //Configure MessageBox Server to accept connection from port 12000
    .AddMessageBoxTcpServer(port: 12000)
    .Build();

serverHost.Run();
```

Now configure the consumer project adding the following code to its program.cs
```c#
using var clientHost = Host.CreateDefaultBuilder()
    .AddMessageBoxTcpClient(System.Net.IPAddress.Loopback, 12000)
    .AddConsumer<SampleConsumer>()
    .AddJsonSerializer()
    .Build();

clientHost.Run();

//This is a consumer class that implements an handler for our event model
class SampleConsumer : IHandler<EventModel>
{
    public Task Handle(IMessageContext<EventModel> messageContext, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Received event from client: {messageContext.Model.Description}");
        return Task.CompletedTask;
    }
}
```
Finally let's add the client code that actually publishes the event model using the ```IBusClient.Publish<T>()``` method:
```c#
using var clientHost = Host.CreateDefaultBuilder()
    .AddMessageBoxTcpClient(System.Net.IPAddress.Loopback, 12000)
    .AddJsonSerializer()
    .Build();

clientHost.Start();

var client = clientHost.Services.GetRequiredService<IBusClient>();

string? eventDesc;
while ((eventDesc = Console.ReadLine()) != null)
{
    await client.Publish(new EventModel(eventDesc));
}
```
Running the apps you should be able to see how messages are published from the client and sent to the consumer. 

Try to run more than one client and/or comsumer to experience how each published message is received by all consumers.

RPC pattern requires instead that each messare has a reply from the consumer. Implementing this patter in MessageBox is easy as well, we just need to change the way client send the message using one of the following method:
1. ```IBusClient.Send<T>()``` to send a message and wait for an ack without a reply message
2. ```IBusClient.SendAndGetReply<T>()``` to send a message and wait for a reply message

So to demostrate how deal with RPC in MessageBox just add a few more models to the shared project like these:
```c#
public record CommandResultModel(int Result);
public record ExecuteCommandModel(int X);
public record ExecuteCommandWithReplyModel(int X);
```

Add two more handlers to the consumer class:
```c#
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
```

Finally replace the while loop in the client project with a code like the below:
```c#
string? valueString;
while ((valueString = Console.ReadLine()) != null)
{
    if (int.TryParse(valueString, out var value))
    {
        //call the consumer and wait until the message is consumed (void-like call)
        await client.Send(new ExecuteCommandModel(value));

        //call the consumer and wait the reply from it
        var reply = await client.SendAndGetReply<CommandResultModel>(new ExecuteCommandWithReplyModel(value));
        Console.WriteLine($"Reply from consumer: {reply}");
    }
}
```

  

