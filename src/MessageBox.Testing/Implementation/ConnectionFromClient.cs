namespace MessageBox.Testing.Implementation
{
    internal class ConnectionFromClient
    {
        private readonly IMessageSink _bus;
        private readonly CancellationTokenSource _cts = new();

        public ConnectionFromClient(IMessageSink bus, IBox box, ClientTransport client)
        {
            _bus = bus;
            Box = box;
            Client = client;
        }

        public IBox Box { get; }

        public ClientTransport Client { get; }

        public async Task ReceiveMessageFromClient(Message message, CancellationToken cancellationToken = default)
        { 
            await _bus.OnReceivedMessage(message with { ReplyToBoxId = Box.Id }, cancellationToken);
        }

        internal async void Start()
        {
            while (!_cts.IsCancellationRequested)
            {
                var message = await Box.GetNextMessageToSend(_cts.Token);

                await Client.OnReceiveMessageFromServer(message, _cts.Token);
            }
        }

        internal void Stop()
        {
            _cts.Cancel();
        }
    }
}
