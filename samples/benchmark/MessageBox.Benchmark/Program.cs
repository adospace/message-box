using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MessageBox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


BenchmarkRunner.Run(typeof(MessageBox.Benchmark.TestBench).Assembly);
//var testBench = new TestBench
//{
//    ConsumerCount = 10,
//    ClientCount = 100,
//    CallCount = 5000,
//};

//testBench.GlobalSetup();
//testBench.Run();
//testBench.GlobalCleanup();

namespace MessageBox.Benchmark
{
    [MemoryDiagnoser]
    public class TestBench
    {
        private IHost _serverHost = null!;
        private Task _serverTask = null!;
        private IHost[] _clientHosts = null!;
        private Task[] _clientTasks = null!;
        private IHost[] _consumerHosts = null!;
        private Task[] _consumerTasks = null!;
        private Task[] _callersTask = null!;

        [Params(100)]
        public int ConsumerCount { get; set; }

        [Params(500)]
        public int ClientCount { get; set; }

        [Params(15000)]
        public int CallCount { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _serverHost = Host.CreateDefaultBuilder()
                .AddMessageBoxTcpServer(port: 12000)
                .Build();

            _serverTask = _serverHost.StartAsync();

            _clientHosts = Enumerable.Range(1, ClientCount).Select(index => 
            {
                return Host.CreateDefaultBuilder()
                    .AddMessageBoxTcpClient(System.Net.IPAddress.Loopback, 12000)
                    .AddJsonSerializer()
                    .Build();
            }).ToArray();

            _clientTasks = _clientHosts.Select(clientHost =>
            {
                return clientHost.StartAsync();
            }).ToArray();

            _consumerHosts = _clientHosts.Select(clientHost =>
            {
                return Host.CreateDefaultBuilder()
                    .AddMessageBoxTcpClient(System.Net.IPAddress.Loopback, 12000)
                    .AddJsonSerializer()
                    .AddConsumer<SampleConsumer>()
                    .Build();
            }).ToArray();

            _consumerTasks = _consumerHosts.Select(consumerHost =>
            {
                return consumerHost.StartAsync();
            }).ToArray();
        
            Random rnd = new();
            _callersTask = _clientHosts.Select(clientHost => Task.Run(async () =>
            {
                var bus = clientHost.Services.GetRequiredService<IBusClient>();
                for (int i = 0; i < CallCount; i++)
                {
                    await bus.SendAndGetReply<SampleCallModelReply>(new SampleCallModel(new string('*', rnd.Next(150)), rnd.Next(0, 123)));
                }
            })).ToArray();
        }

        [Benchmark]
        public void Run()
        {
            Task.WaitAll(new[] { _serverTask }.Concat(_clientTasks).Concat(_consumerTasks).Concat(_callersTask).ToArray());
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            // Disposing logic
            foreach (var clientHost in _clientHosts)
                clientHost.StopAsync().Wait();

            foreach (var consumerHost in _consumerHosts)
                consumerHost.StopAsync().Wait();

            _serverHost.StopAsync().Wait();

            if (SampleConsumer.Calls != ClientCount * CallCount)
            {
                throw new InvalidOperationException();
            }  
        }
    }
    public record SampleCallModel(string StringValue, int IntValue);

    public record SampleCallModelReply(string StringValue, double DoubleValue, DateTime DateTimeValue);

    public class SampleConsumer : IHandler<SampleCallModel, SampleCallModelReply>
    {
        private static readonly Random _rnd = new();

        private static int _calls;
        public static int Calls => _calls;

        public Task<SampleCallModelReply> Handle(IMessageContext<SampleCallModel> messageContext, CancellationToken cancellationToken = default)
        {
            Interlocked.Increment(ref _calls);
            return Task.FromResult(new SampleCallModelReply(new string('*', _rnd.Next(150)), _rnd.Next(), DateTime.MinValue));
        }
    }
}


