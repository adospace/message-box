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

        public class SampleConsumer : IHandler<SampleModel, SampleModelReply>
        {
            public int HandleCallCount { get; private set; }
            public Task<SampleModelReply> Handle(IMessageContext<SampleModel> messageContext, CancellationToken cancellationToken = default)
            {
                HandleCallCount++;
                return Task.FromResult(new SampleModelReply($"Hello {messageContext.Model.Name} {messageContext.Model.Surname}!"));
            }
        }


        [TestMethod]
        public async Task SendAndReceiveMessageUsingTestingFramework()
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
        public async Task SendAndReceiveMessageWithMultipleConsumersUsingTestingFramework()
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
        public async Task PublishEventMessageWithMultipleConsumersUsingTestingFramework()
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

            Assert.AreEqual("Hello John Smith!", reply.NameAndSurname);

            Assert.IsTrue((consumer1.HandleCallCount == 2 && consumer2.HandleCallCount == 1) || (consumer1.HandleCallCount == 1 && consumer2.HandleCallCount == 2));

        }
    }
}