# MessageBox .NET Message Broker
MessageBox is a .NET 6 message broker that aims to simplify message communication between services in a microservice architecture. 
It's entirely written in .NET/C# and tries to use all the best practices and latest features of the framework in order to reduce memory consumption and CPU usage while leverage optimal perfomance for many use cases. 
It's not a full fledged product nor it aims to be a one tool for every scenario instead it tries to make message interprocess communication easy to develop and test.

[![Build status](https://ci.appveyor.com/api/projects/status/dkyae4p5jagnu3k4?svg=true)](https://ci.appveyor.com/project/adospace/message-box)

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
  
  You can also integrate exactly the same the brocker service in a ASP.NET core project
  
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
Finally let's add the client code that actually publishes the event model
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


