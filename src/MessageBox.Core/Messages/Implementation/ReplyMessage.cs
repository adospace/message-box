using System.Buffers;

namespace MessageBox.Messages.Implementation
{
    internal class ReplyMessage : IReplyMessage
    {
        public ReplyMessage(ICallMessage message)
        {
            Id = Guid.NewGuid();
            CorrelationId = Guid.NewGuid();
            ReplyToId = message.Id;
        }

        public Guid Id { get; }

        public Guid CorrelationId { get; }

        public Guid ReplyToId { get; }

        public void Serialize(IBufferWriter<byte> writer)
        {
            throw new NotImplementedException();
        }
    }
}