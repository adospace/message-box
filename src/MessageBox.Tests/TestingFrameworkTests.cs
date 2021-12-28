using System.Linq;
using MessageBox.Testing;
using MessageBox.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MessageBox.Server;

namespace MessageBox.Tests
{
    [TestClass]
    public class TestingFrameworkTests
    {
        [TestMethod]
        public async Task SendAndReceiveMessage()
        {
            using var serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .Build();

            using var consumerHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .AddConsumer<SampleConsumer>()
                .Build();

            await serverHost.StartAsync();
            await clientHost.StartAsync();
            await consumerHost.StartAsync();

            var busClient = clientHost.Services.GetRequiredService<IBusClient>();
            var reply = await busClient.SendAndGetReply<SampleModelReply>(new SampleModel("John", "Smith"));

            Assert.AreEqual("Hello John Smith!", reply.NameAndSurname);
        }

        [TestMethod]
        public async Task SendAndReceiveMessageWithMultipleConsumers()
        {
            using var serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .Build();

            var consumer1 = new SampleConsumer();
            using var consumerHost1 = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .AddConsumer(consumer1)
                .Build();

            var consumer2 = new SampleConsumer();
            using var consumerHost2 = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .AddConsumer(consumer2)
                .Build();

            await serverHost.StartAsync();
            await clientHost.StartAsync();
            await consumerHost1.StartAsync();
            await consumerHost2.StartAsync();

            var busClient = clientHost.Services.GetRequiredService<IBusClient>();
            var reply = await busClient.SendAndGetReply<SampleModelReply>(new SampleModel("John", "Smith"));

            Assert.AreEqual("Hello John Smith!", reply.NameAndSurname);

            WaitHandle.WaitAny(new WaitHandle[] { consumer1.HandleCalled, consumer2.HandleCalled });
        }

        [TestMethod]
        public async Task PublishEventMessageWithMultipleConsumers()
        {
            using var serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .Build();

            var consumer1 = new SampleConsumer();
            using var consumerHost1 = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .AddConsumer(consumer1)
                .Build();

            var consumer2 = new SampleConsumer();
            using var consumerHost2 = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .AddConsumer(consumer2)
                .Build();

            await serverHost.StartAsync();
            await clientHost.StartAsync();
            await consumerHost1.StartAsync();
            await consumerHost2.StartAsync();

            var busClient = clientHost.Services.GetRequiredService<IBusClient>();

            await busClient.Publish(new SampleModel("John", "Smith"));

            foreach (var ev in new WaitHandle[] { consumer1.HandleCalled, consumer2.HandleCalled })
                ev.WaitOne();
        }

        [TestMethod]
        public async Task SendMessageWithMultipleConsumers()
        {
            using var serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .Build();

            var consumer1 = new SampleConsumer();
            using var consumerHost1 = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .AddConsumer(consumer1)
                .Build();

            var consumer2 = new SampleConsumer();
            using var consumerHost2 = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .AddConsumer(consumer2)
                .Build();

            await serverHost.StartAsync();
            await clientHost.StartAsync();
            await consumerHost1.StartAsync();
            await consumerHost2.StartAsync();

            var busClient = clientHost.Services.GetRequiredService<IBusClient>();
            await busClient.Send(new SampleModel("John", "Smith"));

            WaitHandle.WaitAny(new WaitHandle[] { consumer1.HandleCalled, consumer2.HandleCalled });
        }

        [TestMethod]
        public async Task SendAndReceiveMessageWhenConsumerIsAvailable()
        {
            using var serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .Build();

            await serverHost.StartAsync();
            await clientHost.StartAsync();

            var busClient = clientHost.Services.GetRequiredService<IBusClient>();
            var replyTask = busClient.SendAndGetReply<SampleModelReply>(new SampleModel("John", "Smith"));
            var startConsumerHostTask = Task.Run(async () =>
            {
                using var consumerHost = Host.CreateDefaultBuilder()
                    .AddMessageBoxInMemoryClient()
                    .AddJsonSerializer()
                    .AddConsumer<SampleConsumer>()
                    .Build();
                
                await Task.Delay(2000);
                
                await consumerHost.StartAsync();
                
                await Task.Delay(2000);
            });

            Task.WaitAll(replyTask, startConsumerHostTask);

            Assert.AreEqual("Hello John Smith!", replyTask.Result.NameAndSurname);
        }

        [TestMethod]
        public async Task SendAndConsumerThrowsException()
        {
            using var serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .Build();

            using var consumerHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .AddConsumer<SampleConsumer>()
                .Build();

            await serverHost.StartAsync();
            await clientHost.StartAsync();
            await consumerHost.StartAsync();

            var busClient = clientHost.Services.GetRequiredService<IBusClient>();
            await Assert.ThrowsExceptionAsync<MessageBoxCallException>(() => busClient.Send(new SampleModelThatRaisesException()));
        }

        [TestMethod]
        public async Task SetQueueNameAndCheck()
        {
            using var serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient(new InMemoryBusClientOptions()
                {
                    Name = "queue_name"
                })
                .AddJsonSerializer()
                .Build();

            await serverHost.StartAsync();
            await clientHost.StartAsync();

            await Task.Delay(1000);
            
            var busServerControl = serverHost.Services.GetRequiredService<IBusServerControl>();
            var queue = busServerControl.GetQueues().FirstOrDefault(_ => _.Name == "queue_name");
            queue.Should().NotBeNull();
        }
    }
}