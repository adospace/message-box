using System.Buffers;

namespace MessageBox.Messages.Implementation;

internal class KeepAliveMessage : IKeepAliveMessage, ISerializableMessage
{
    public Guid QueueId { get; }

    public KeepAliveMessage(Guid queueId)
    {
        QueueId = queueId;
    }

    public void Serialize(IBufferWriter<byte> writer)
    {
        var buffer = writer.GetMemory(1 + 16);
        var binaryWriter = new MemoryByteBinaryWriter(buffer);
        binaryWriter.Write((byte)MessageType.KeepAliveMessage);
        binaryWriter.Write(QueueId);

        writer.Advance(1 + 16);
    }
    
    public static void TryDeserialize(ref ReadOnlySequence<byte> buffer, out IMessage? message)
    {
        message = null;

        var messageLength = 16;

        if (buffer.Length < 1 + messageLength)
        {
            return;
        }

        var memoryOwner = MemoryPool<byte>.Shared.Rent(messageLength);

        buffer.Slice(1, messageLength).CopyTo(memoryOwner.Memory.Span);

        var reader = new MemoryByteBinaryReader(memoryOwner.Memory);
        message = new KeepAliveMessage(
            queueId: reader.ReadGuid());

        buffer = buffer.Slice(1 + messageLength);
    }
}