using System.Buffers;
using System.Runtime.InteropServices;

namespace MessageBox.Messages.Implementation
{
    internal class SubsribeMessage : ISubscribeMessage
    {
        public SubsribeMessage(string exchangeName)
        {
            ExchangeName = exchangeName;
        }

        public string ExchangeName { get; }

        public virtual void Serialize(IBufferWriter<byte> writer)
        {
            var binaryWriterEstimator = new MemoryByteBinaryWriter();
            binaryWriterEstimator.Write(ExchangeName);

            var messageLength = binaryWriterEstimator.Offset;

            var buffer = writer.GetMemory(1 + messageLength);
            var binaryWriter = new MemoryByteBinaryWriter(buffer);
            binaryWriter.Write((byte)MessageType.SubsribeMessage);
            binaryWriter.Write(ExchangeName);

            writer.Advance(1 + messageLength);
        }
        public static void TryDeserialize(ref ReadOnlySequence<byte> buffer, out IMessage? message)
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

            message = new SubsribeMessage(
                exchangeName: reader.ReadString(messageLength));

            buffer = buffer.Slice(5 + messageLength);
        }

    }
}