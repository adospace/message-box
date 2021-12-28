using MessageBox.Messages;
using System.Threading.Channels;

namespace MessageBox.Server.Implementation
{
    internal class Queue : IQueue, IQueueControl
    {
        private class MessageEntry
        {
            private readonly DateTime _arrived;
            
            public MessageEntry(ITransportMessage message)
            {
                Message = message;
                _arrived = DateTime.UtcNow;
            }

            public ITransportMessage Message { get; }

            public bool IsExpired => (_arrived - DateTime.UtcNow).TotalSeconds > Message.TimeToLiveSeconds;
        }

        private readonly Channel<MessageEntry> _outgoingMessages = Channel.CreateUnbounded<MessageEntry>();

        public Queue(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
        
        public string? Name { get; private set; }
        
        public int GetMessageCount()
        {
            return _outgoingMessages.Reader.Count;
        }

        public async Task OnReceivedMessage(IMessage message, CancellationToken cancellationToken)
        {
            if (message is ISetQueueNameMessage setQueueNameMessage)
            {
                Name = setQueueNameMessage.SetQueueName;
                return;
            }
            
            var messageEntry = new MessageEntry((ITransportMessage)message);
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
