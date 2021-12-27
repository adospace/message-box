using System.Buffers;
using System.Runtime.InteropServices;

namespace MessageBox.Messages.Implementation
{
    internal sealed class ReplyWithPayloadMessage : ReplyMessage, IReplyWithPayloadMessage
    {
        private bool _disposedValue;
        private readonly IMemoryOwner<byte>? _memoryOwner;

        public ReplyWithPayloadMessage(ICallQueuedMessage message, string payloadType, ReadOnlyMemory<byte> payload)
            : base(message)
        {
            PayloadType = payloadType;
            Payload = payload;
        }

        private ReplyWithPayloadMessage(Guid id, Guid correlationId, Guid replyToId, Guid replyToBoxId, int timeToLiveSeconds, string payloadType, ReadOnlyMemory<byte> payload, IMemoryOwner<byte> memoryOwner) 
            : base(id, correlationId, replyToId, replyToBoxId, timeToLiveSeconds)
        {
            PayloadType = payloadType;
            Payload = payload;
            _memoryOwner = memoryOwner;
        }

        public string PayloadType { get; }

        public ReadOnlyMemory<byte> Payload { get; }

        public override void Serialize(IBufferWriter<byte> writer)
        {
            var binaryWriterEstimator = new MemoryByteBinaryWriter();
            binaryWriterEstimator.Write(Id);
            binaryWriterEstimator.Write(CorrelationId);
            binaryWriterEstimator.Write(ReplyToId);
            binaryWriterEstimator.Write(ReplyToBoxId);
            binaryWriterEstimator.Write(TimeToLiveSeconds);
            binaryWriterEstimator.Write(PayloadType);
            binaryWriterEstimator.WriteRemainingBuffer(Payload);

            var messageLength = binaryWriterEstimator.Offset;

            var buffer = writer.GetMemory(5 + messageLength);
            var binaryWriter = new MemoryByteBinaryWriter(buffer);
            binaryWriter.Write((byte)MessageType.ReplyWithPayloadMessage);
            binaryWriter.Write(messageLength);
            binaryWriter.Write(Id);
            binaryWriter.Write(CorrelationId);
            binaryWriter.Write(ReplyToId);
            binaryWriter.Write(ReplyToBoxId);
            binaryWriter.Write(TimeToLiveSeconds);
            binaryWriter.Write(PayloadType);
            binaryWriter.WriteRemainingBuffer(Payload);

            writer.Advance(5 + messageLength);
        }

        public new static void TryDeserialize(ref ReadOnlySequence<byte> buffer, out IMessage? message)
        {
            message = null;

            if (buffer.Length < 5)
            {
                return;
            }

            var messageLength = MemoryMarshal.Read<int>(buffer.Slice(1, 4).ToArray());

            if (buffer.Length < 5 + messageLength)
            {
                return;
            }

            var memoryOwner = MemoryPool<byte>.Shared.Rent(messageLength);

            buffer.Slice(5, messageLength).CopyTo(memoryOwner.Memory.Span);

            var reader = new MemoryByteBinaryReader(memoryOwner.Memory);
            
            message = new ReplyWithPayloadMessage(
                id: reader.ReadGuid(),
                correlationId: reader.ReadGuid(),
                replyToId: reader.ReadGuid(),
                replyToBoxId: reader.ReadGuid(),
                timeToLiveSeconds: reader.ReadInt32(),
                payloadType: reader.ReadString(),
                payload: reader.ReadRemainingBuffer(messageLength),
                memoryOwner: memoryOwner);

            buffer = buffer.Slice(5 + messageLength);
        }

        public void Dispose()
        {
            if (_disposedValue) return;
            
            _memoryOwner?.Dispose();

            _disposedValue = true;
        }
    }
}