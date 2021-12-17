namespace MessageBox.Testing.Implementation
{
    internal class ConnectionFromClient
    {
        private readonly IMessageSink _bus;
        private readonly CancellationTokenSource _cts = new();

        public ConnectionFromClient(IMessageSink bus, IQueue queue, ClientTransport client)
        {
            _bus = bus;
            Queue = queue;
            Client = client;
        }

        public IQueue Queue { get; }

        public ClientTransport Client { get; }

        public async Task ReceiveMessageFromClient(Message message, CancellationToken cancellationToken = default)
        { 
            await _bus.OnReceivedMessage(message.ReplyToBoxId != null ? message : message with { ReplyToBoxId = Queue.Id }, cancellationToken);
        }

        internal async void Start()
        {
            while (!_cts.IsCancellationRequested)
            {
                var message = await Queue.GetNextMessageToSend(_cts.Token);

                await Client.OnReceiveMessageFromServer(message, _cts.Token);
            }
        }

        internal void Stop()
        {
            _cts.Cancel();
        }
    }
}
