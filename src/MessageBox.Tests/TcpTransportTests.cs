using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MessageBox.Tests
{
    [TestClass]
    public class TcpTransportTests
    {
        private record SampleModel(string Name, string Surname);

        private record SampleModelReply(string NameAndSurname);

        private record SampleModelThatRaisesException;

        private class SampleConsumer :
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
            using var serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpServer(12000)
                .ConfigureLogging((_, logging) => 
                {
                    logging.SetMinimumLevel(LogLevel.Trace);
                })
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12000)
                .AddJsonSerializer()
                .Build();

            using var consumerHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12000)
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
                .AddMessageBoxTcpServer(12001)
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12001)
                .AddJsonSerializer()
                .Build();

            var consumer1 = new SampleConsumer();
            using var consumerHost1 = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12001)
                .AddJsonSerializer()
                .AddConsumer(consumer1)
                .Build();

            var consumer2 = new SampleConsumer();
            using var consumerHost2 = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12000)
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
            using var serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpServer(12002)
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12002)
                .AddJsonSerializer()
                .Build();

            var consumer1 = new SampleConsumer();
            using var consumerHost1 = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12002)
                .AddJsonSerializer()
                .AddConsumer(consumer1)
                .Build();

            var consumer2 = new SampleConsumer();
            using var consumerHost2 = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12002)
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

            await Task.Delay(2000);

            Assert.AreEqual("Hello John Smith!", reply.NameAndSurname);

            Assert.IsTrue((consumer1.HandleCallCount == 2 && consumer2.HandleCallCount == 1) || (consumer1.HandleCallCount == 1 && consumer2.HandleCallCount == 2));

        }

        [TestMethod]
        public async Task SendMessageWithMultipleConsumers()
        {
            using var serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpServer(12003)
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12003)
                .AddJsonSerializer()
                .Build();

            var consumer1 = new SampleConsumer();
            using var consumerHost1 = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12003)
                .AddJsonSerializer()
                .AddConsumer(consumer1)
                .Build();

            var consumer2 = new SampleConsumer();
            using var consumerHost2 = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12003)
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
            using var serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpServer(12004)
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12004)
                .AddJsonSerializer()
                .Build();


            await serverHost.StartAsync();
            await clientHost.StartAsync();

            var busClient = clientHost.Services.GetRequiredService<IBusClient>();
            var replyTask = busClient.SendAndGetReply<SampleModelReply>(new SampleModel("John", "Smith"));
            var startConsumerHostTask = Task.Run(async () =>
            {
                using var consumerHost = Host.CreateDefaultBuilder()
                    .AddMessageBoxTcpClient(IPAddress.Loopback, 12004)
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
                .AddMessageBoxTcpServer(12005)
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12005)
                .AddJsonSerializer()
                .Build();

            using var consumerHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12005)
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
