# MessageBox .NET Message Broker
MessageBox is a .NET 6 message broker that aims to simplify message communication between services in a microservice architecture. 
It's entirely written in .NET/C# and tries to use all the best practices and latest features of the framework in order to reduce memory consumption and CPU usage while leaverange optimal perfomance for many use cases. 
It's not a full fladged product nor aims to be a one tool for every scenario but tries to make message interprocess communication easy to develop and test.

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
  

  
