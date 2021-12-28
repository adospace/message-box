using MessageBox.Messages;

namespace MessageBox.Testing.Implementation
{
    internal class ConnectionFromClient
    {
        private readonly IMessageFactory _messageFactory;
        private readonly IMessageSink _bus;
        private readonly IQueue _queue;
        private readonly ClientTransport _client;
        private readonly CancellationTokenSource _cts = new();

        public ConnectionFromClient(IMessageFactory messageFactory, IMessageSink bus, IQueue queue, ClientTransport client)
        {
            _messageFactory = messageFactory;
            _bus = bus;
            _queue = queue;
            _client = client;
        }

        public Task OnReceivedMessage(IMessage message, CancellationToken cancellationToken = default)
        {
            return message switch
            {
                ICallMessage callMessage => _bus.OnReceivedMessage(
                    _messageFactory.CreateCallQueuedMessage(callMessage, _queue.Id), cancellationToken),
                ISubscribeMessage subscribeToExchangeMessage => _bus.OnReceivedMessage(
                    _messageFactory.CreateSubsribeQueuedMessage(subscribeToExchangeMessage, _queue.Id),
                    cancellationToken),
                ISetQueueNameMessage setQueueNameMessage
                    => _bus.OnReceivedMessage(
                        _messageFactory.CreateSetQueueNameQueuedMessage(setQueueNameMessage.SetQueueName, _queue.Id),
                        cancellationToken),
                _ => _bus.OnReceivedMessage(message, cancellationToken)
            };
        }

        internal async void Start()
        {
            while (!_cts.IsCancellationRequested)
            {
                var message = await _queue.GetNextMessageToSend(_cts.Token);

                await _client.OnReceiveMessageFromServer(message, _cts.Token);
            }
        }

        internal void Stop()
        {
            _cts.Cancel();
        }
    }
}
