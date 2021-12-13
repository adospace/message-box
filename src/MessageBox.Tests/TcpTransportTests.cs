using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MessageBox.Tests
{
    [TestClass]
    public class TcpTransportTests
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
        public async Task SendAndReceiveMessage()
        {
            using IHost serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpServer(12000)
                .Build();

            using IHost clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpClient(IPAddress.Loopback, 12000)
                .AddJsonSerializer()
                .Build();

            using IHost consumerHost = Host.CreateDefaultBuilder()
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

    }
}
