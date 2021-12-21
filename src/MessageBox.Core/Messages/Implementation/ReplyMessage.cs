using System.Buffers;
using System.Runtime.InteropServices;

namespace MessageBox.Messages.Implementation
{
    internal class ReplyMessage : IReplyMessage
    {
        public ReplyMessage(ICallQueuedMessage message)
        {
            Id = Guid.NewGuid();
            CorrelationId = Guid.NewGuid();
            ReplyToId = message.Id;
            ReplyToBoxId = message.SourceQueueId;
        }
        protected ReplyMessage(Guid id, Guid correlationId, Guid replyToId, Guid replyToBoxId)
        {
            Id = id;
            CorrelationId = correlationId;
            ReplyToId = replyToId;
            ReplyToBoxId = replyToBoxId;
        }

        public Guid Id { get; }

        public Guid CorrelationId { get; }

        public Guid ReplyToId { get; }

        public Guid ReplyToBoxId { get; }

        public virtual void Serialize(IBufferWriter<byte> writer)
        {
            var buffer = writer.GetMemory(1 + 16 * 4);
            var binaryWriter = new MemoryByteBinaryWriter(buffer);
            binaryWriter.Write((byte)MessageType.ReplyMessage);
            binaryWriter.Write(Id);
            binaryWriter.Write(CorrelationId);
            binaryWriter.Write(ReplyToId);
            binaryWriter.Write(ReplyToBoxId);

            writer.Advance(1 + 16 * 4);
        }

        public static void TryDeserialize(ref ReadOnlySequence<byte> buffer, out IMessage? message)
        {
            message = null;

            var messageLength = 16 * 4;

            if (buffer.Length < 1 + messageLength)
            {
                return;
            }

            var memoryOwner = MemoryPool<byte>.Shared.Rent(messageLength);

            buffer.Slice(1, messageLength).CopyTo(memoryOwner.Memory.Span);

            var reader = new MemoryByteBinaryReader(memoryOwner.Memory);
            message = new ReplyMessage(
                id: reader.ReadGuid(),
                correlationId: reader.ReadGuid(),
                replyToId: reader.ReadGuid(),
                replyToBoxId: reader.ReadGuid());

            buffer = buffer.Slice(1 + messageLength);
        }
    }
}