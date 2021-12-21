using MessageBox.Testing;
using MessageBox.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBox.Tests
{
    [TestClass]
    public partial class TestingFrameworkTests
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

            WaitHandle.WaitAny(new[] { consumer1.HandleCalled, consumer2.HandleCalled });
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

            WaitHandle.WaitAll(new[] { consumer1.HandleCalled, consumer2.HandleCalled });

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

            WaitHandle.WaitAny(new[] { consumer1.HandleCalled, consumer2.HandleCalled });
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

    }
}