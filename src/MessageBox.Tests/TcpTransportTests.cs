using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MessageBox.Server;
using Microsoft.Extensions.Logging;
using MessageBox.Tests.Helpers;

namespace MessageBox.Tests
{
    [TestClass]
    public class TcpTransportTests
    {
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

            foreach (var ev in new WaitHandle[] { consumer1.HandleCalled, consumer2.HandleCalled })
                ev.WaitOne();
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

        
        [TestMethod]
        public async Task SetQueueNameAndCheck()
        {
            using var serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpServer(12006)
                .Build();

            using var clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(new TcpBusClientOptions(IPAddress.Loopback, 12006)
                {
                    Name = "queue_name"
                })
                .AddJsonSerializer()
                .Build();

            await serverHost.StartAsync();
            await clientHost.StartAsync();

            await Task.Delay(1000);
            
            var busServerControl = serverHost.Services.GetRequiredService<IBusServerControl>();
            var queue = busServerControl.GetQueues().FirstOrDefault(_ => _.Name.StartsWith("queue_name"));
            queue.Should().NotBeNull();
        }
    }
}
