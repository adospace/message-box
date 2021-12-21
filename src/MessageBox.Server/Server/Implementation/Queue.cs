using MessageBox.Messages;
using System.Threading.Channels;

namespace MessageBox.Server.Implementation
{
    internal class Queue : IQueue
    {
        private readonly Channel<IMessage> _outgoingMessages = Channel.CreateUnbounded<IMessage>();

        public Queue(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }

        public async Task OnReceivedMessage(IMessage message, CancellationToken cancellationToken)
        { 
            await _outgoingMessages.Writer.WriteAsync(message, cancellationToken);
        }

        public async Task<IMessage> GetNextMessageToSend(CancellationToken cancellationToken)
        {
            return await _outgoingMessages.Reader.ReadAsync(cancellationToken);
        }
    }
}
