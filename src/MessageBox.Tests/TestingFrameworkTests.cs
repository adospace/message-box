using MessageBox.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBox.Tests
{
    [TestClass]
    public class TestingFrameworkTests
    {
        public record SampleModel(string Name, string Surname);
        public record SampleModelReply(string NameAndSurname);

        public record SampleModelThatRaisesException();

        public class SampleConsumer : 
            IHandler<SampleModel, SampleModelReply>,
            IHandler<SampleModelThatRaisesException>
        {
            public int HandleCallCount { get; private set; }
            public Task<SampleModelReply> Handle(IMessageContext<SampleModel> messageContext, CancellationToken cancellationToken = default)
            {
                HandleCallCount++;
                return Task.FromResult(new SampleModelReply($"Hello {messageContext.Model.Name} {messageContext.Model.Surname}!"));
            }

            public Task Handle(IMessageContext<SampleModelThatRaisesException> messageContext, CancellationToken cancellationToken = default)
            {
                throw new System.NotImplementedException();
            }
        }


        [TestMethod]
        public async Task SendAndReceiveMessage()
        {
            using IHost serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using IHost clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .Build();

            using IHost consumerHost = Host.CreateDefaultBuilder()
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
            using IHost serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using IHost clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .Build();

            var consumer1 = new SampleConsumer();
            using IHost consumerHost1 = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .AddConsumer(consumer1)
                .Build();

            var consumer2 = new SampleConsumer();
            using IHost consumerHost2 = Host.CreateDefaultBuilder()
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

            Assert.IsTrue((consumer1.HandleCallCount == 1 && consumer2.HandleCallCount == 0) || (consumer1.HandleCallCount == 0 && consumer2.HandleCallCount == 1));
        }

        [TestMethod]
        public async Task PublishEventMessageWithMultipleConsumers()
        {
            using IHost serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using IHost clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .Build();

            var consumer1 = new SampleConsumer();
            using IHost consumerHost1 = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .AddConsumer(consumer1)
                .Build();

            var consumer2 = new SampleConsumer();
            using IHost consumerHost2 = Host.CreateDefaultBuilder()
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

            var reply = await busClient.SendAndGetReply<SampleModelReply>(new SampleModel("John", "Smith"));

            await Task.Delay(1000);

            Assert.AreEqual("Hello John Smith!", reply.NameAndSurname);

            Assert.IsTrue((consumer1.HandleCallCount == 2 && consumer2.HandleCallCount == 1) || (consumer1.HandleCallCount == 1 && consumer2.HandleCallCount == 2));

        }

        [TestMethod]
        public async Task SendMessageWithMultipleConsumers()
        {
            using IHost serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using IHost clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .Build();

            var consumer1 = new SampleConsumer();
            using IHost consumerHost1 = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .AddConsumer(consumer1)
                .Build();

            var consumer2 = new SampleConsumer();
            using IHost consumerHost2 = Host.CreateDefaultBuilder()
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

            Assert.IsTrue((consumer1.HandleCallCount == 1 && consumer2.HandleCallCount == 0) || (consumer1.HandleCallCount == 0 && consumer2.HandleCallCount == 1));
        }


        [TestMethod]
        public async Task SendAndReceiveMessageWhenConsumerIsAvailable()
        {
            using IHost serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using IHost clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .Build();

            using IHost consumerHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .AddConsumer<SampleConsumer>()
                .Build();

            await serverHost.StartAsync();
            await clientHost.StartAsync();

            var busClient = clientHost.Services.GetRequiredService<IBusClient>();
            var replyTask = busClient.SendAndGetReply<SampleModelReply>(new SampleModel("John", "Smith"));
            var startConsumerHostTask = Task.Run(async () =>
            {
                await Task.Delay(4000);
                await consumerHost.StartAsync();
            });

            Task.WaitAll(replyTask, startConsumerHostTask);

            Assert.AreEqual("Hello John Smith!", replyTask.Result.NameAndSurname);
        }

        [TestMethod]
        public async Task SendAndConsumerThrowsException()
        {
            using IHost serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using IHost clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .Build();

            using IHost consumerHost = Host.CreateDefaultBuilder()
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