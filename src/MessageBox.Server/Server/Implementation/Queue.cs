using MessageBox.Messages;
using System.Threading.Channels;

namespace MessageBox.Server.Implementation
{
    internal class Queue : IQueue, IQueueControl
    {
        private readonly string _key;
        private readonly IMessageFactory _messageFactory;
        private int _messageCount;

        private class MessageEntry
        {
            private readonly int _timeToLiveSeconds;
            private readonly DateTime _arrived;
            
            public MessageEntry(IMessage message, int timeToLiveSeconds)
            {
                _timeToLiveSeconds = timeToLiveSeconds;
                Message = message;
                _arrived = DateTime.UtcNow;
            }
            
            public MessageEntry(ITransportMessage message)
            {
                _timeToLiveSeconds = message.TimeToLiveSeconds;
                Message = message;
                _arrived = DateTime.UtcNow;
            }

            public IMessage Message { get; }

            public bool IsExpired => (_arrived - DateTime.UtcNow).TotalSeconds > _timeToLiveSeconds;
        }

        private readonly Channel<MessageEntry> _outgoingMessages = Channel.CreateUnbounded<MessageEntry>();
        private string? _name;

        public Queue(Guid id, string key, IMessageFactory messageFactory)
        {
            _key = key;
            _messageFactory = messageFactory;
            Id = id;
        }

        public Guid Id { get; }

        public string Name => _name == null ? _key : $"{_name}({_key})";

        public int GetTotalMessageCount()
        {
            return _messageCount;
        }

        public int GetCurrentMessageCount()
        {
            return _outgoingMessages.Reader.Count;
        }

        public async Task TryToKeepAlive(CancellationToken cancellationToken)
        {
            await _outgoingMessages.Writer.WriteAsync(new MessageEntry(_messageFactory.CreateKeepAliveMessage(Id), int.MaxValue), cancellationToken);
        }

        public async Task OnReceivedMessage(IMessage message, CancellationToken cancellationToken)
        {
            switch (message)
            {
                case ISetQueueNameMessage setQueueNameMessage:
                    _name = setQueueNameMessage.SetQueueName;
                    return;
                case IKeepAliveMessage:
                    return;
            }

            var transportMessage = (ITransportMessage)message;
            var messageEntry = new MessageEntry(transportMessage);
            if (messageEntry.IsExpired)
            {
                return;
            }

            _messageCount++;
            await _outgoingMessages.Writer.WriteAsync(messageEntry, cancellationToken);
        }

        public async Task<IMessage> GetNextMessageToSend(CancellationToken cancellationToken)
        {
            while (true)
            {
                var messageEntry = await _outgoingMessages.Reader.ReadAsync(cancellationToken);
                if (!messageEntry.IsExpired)
                {
                    return messageEntry.Message;
                }
            }
        }

        public void Stop()
        {
            _outgoingMessages.Writer.Complete();
        }
    }
}
