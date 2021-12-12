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
            public Task<SampleModelReply> Handle(IMessageContext<SampleModel> messageContext, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new SampleModelReply($"Hello {messageContext.Model.Name} {messageContext.Model.Surname}!"));
            }
        }


        [TestMethod]
        public async Task PublishAndReceiveMessageUsingTestingFramework()
        {
            using IHost serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryServer()
                .Build();

            using IHost clientHost = Host.CreateDefaultBuilder()
                .AddMessageBoxInMemoryClient()
                .AddJsonSerializer()
                .ConfigureServices((ctx, services) => services.AddConsumer<SampleConsumer>())
                .Build();

            await serverHost.StartAsync();
            await clientHost.StartAsync();

            var busClient = clientHost.Services.GetRequiredService<IBusClient>();
            var reply = await busClient.SendAndGetReply<SampleModel, SampleModelReply>(new SampleModel("John", "Smith"));

            Assert.AreEqual("Hello John Smith!", reply.NameAndSurname);
        }
    }
}