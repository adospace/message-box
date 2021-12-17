using System.Threading.Channels;

namespace MessageBox.Server.Implementation
{
    internal class Queue : IQueue
    {
        private readonly Channel<Message> _outgoingMessages = Channel.CreateUnbounded<Message>();

        public Queue(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }

        public async Task OnReceivedMessage(Message message, CancellationToken cancellationToken)
        { 
            await _outgoingMessages.Writer.WriteAsync(message, cancellationToken);
        }

        public async Task<Message> GetNextMessageToSend(CancellationToken cancellationToken)
        {
            return await _outgoingMessages.Reader.ReadAsync(cancellationToken);
        }
    }
}
