using MessageBox.Messages;
using System.Threading.Channels;

namespace MessageBox.Server.Implementation
{
    internal class Queue : IQueue, IQueueControl
    {
        private readonly IMessageFactory _messageFactory;

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
        private DateTime? _lastReceivedMessageTimeStamp;

        public Queue(Guid id, IMessageFactory messageFactory)
        {
            _messageFactory = messageFactory;
            Id = id;
        }

        public Guid Id { get; }
        
        public string? Name { get; private set; }
        
        public int GetMessageCount()
        {
            return _outgoingMessages.Reader.Count;
        }

        public async Task<bool> IsAlive(TimeSpan keepAliveTimeout, CancellationToken cancellationToken)
        {
            if ((DateTime.UtcNow - _lastReceivedMessageTimeStamp) > keepAliveTimeout)
            {
                return false;
            }

            await _outgoingMessages.Writer.WriteAsync(new MessageEntry(_messageFactory.CreateKeepAliveMessage(Id), int.MaxValue), cancellationToken);
            
            return true;
        }

        public async Task OnReceivedMessage(IMessage message, CancellationToken cancellationToken)
        {
            _lastReceivedMessageTimeStamp = DateTime.UtcNow;
            
            switch (message)
            {
                case ISetQueueNameMessage setQueueNameMessage:
                    Name = setQueueNameMessage.SetQueueName;
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
    }
}
