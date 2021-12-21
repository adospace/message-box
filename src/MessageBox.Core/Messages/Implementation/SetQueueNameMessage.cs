using System.Buffers;
using System.Runtime.InteropServices;

namespace MessageBox.Messages.Implementation
{
    internal class SetQueueNameMessage : ISetQueueNameMessage
    {
        public SetQueueNameMessage(string queueName)
        {
            SetQueueName = queueName;
        }

        public string SetQueueName { get; }

        public void Serialize(IBufferWriter<byte> writer)
        {
            var binaryWriterEstimator = new MemoryByteBinaryWriter();
            binaryWriterEstimator.Write(SetQueueName);

            var messageLength = binaryWriterEstimator.Offset;

            var buffer = writer.GetMemory(1 + messageLength);
            var binaryWriter = new MemoryByteBinaryWriter(buffer);
            binaryWriter.Write((byte)MessageType.SetQueueNameMessage);
            binaryWriter.Write(SetQueueName);

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

            message = new SetQueueNameMessage(
                queueName: reader.ReadString(messageLength));

            buffer = buffer.Slice(5 + messageLength);
        }

    }
}