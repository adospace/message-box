using System.Buffers;
using System.Runtime.InteropServices;

namespace MessageBox
{
    //public record Message(
    //    /// <summary>
    //    /// Unique Id for the message
    //    /// </summary>
    //    Guid Id,

    //    /// <summary>
    //    /// Id of the message whose this message is a reply to
    //    /// </summary>
    //    Guid? ReplyToId = null,

    //    /// Indicates if this message require a replay (is the first message in a RPC call)
    //    bool RequireReply = false,

    //    //Indicates if this is an event that should be broacasted
    //    bool IsEvent = false,

    //    // Indicates if this message is an ack to the message with this Id
    //    bool IsAck = false,

    //    /// <summary>
    //    /// Unique Key of the board where this message is published to
    //    /// </summary>
    //    string? BoardKey = null,

    //    /// <summary>
    //    /// Unique Id of the box where the reply to this message should be send
    //    /// </summary>
    //    Guid? ReplyToBoxId = null,

    //    /// <summary>
    //    /// Optional correlation id of the message, useful to "correlate" different messages
    //    /// </summary>
    //    Guid? CorrelationId = null,

    //    /// <summary>
    //    /// Type name of the object serialized in the Payload property
    //    /// </summary>
    //    string? PayloadType = null,

    //    /// <summary>
    //    /// Optional payload data
    //    /// </summary>
    //    ReadOnlyMemory<byte>? Payload = null,

    //    IMemoryOwner<byte>? MessageMemoryOwner = null
    //)
    //{
    //    public static bool TryDeserialize(ref ReadOnlySequence<byte> buffer, out Message? message)
    //    {
    //        message = null;

    //        if (buffer.Length < 4)
    //        {
    //            return false;
    //        }

    //        var messageLength = MemoryMarshal.Read<int>(buffer.Slice(0, 4).ToArray());

    //        if (buffer.Length < 4 + messageLength)
    //        {
    //            return false;
    //        }

    //        var memoryOwner = MemoryPool<byte>.Shared.Rent(messageLength);

    //        buffer.Slice(4, messageLength).CopyTo(memoryOwner.Memory.Span);

    //        var reader = new MemoryByteBinaryReader(memoryOwner.Memory);

    //        message = new Message(
    //            Id: reader.ReadGuid(),
    //            ReplyToId: reader.ReadNullableGuid(),
    //            RequireReply: reader.ReadBoolean(),
    //            IsEvent: reader.ReadBoolean(),
    //            IsAck: reader.ReadBoolean(),
    //            BoardKey: reader.ReadNullableString(),
    //            ReplyToBoxId: reader.ReadNullableGuid(),
    //            CorrelationId: reader.ReadNullableGuid(),
    //            PayloadType: reader.ReadNullableString(),
    //            Payload: reader.ReadRemainingBuffer(messageLength),
    //            MessageMemoryOwner: memoryOwner);

    //        buffer = buffer.Slice(4 + messageLength);

    //        return message != null;
    //    }

    //    public void Serialize(IBufferWriter<byte> writer)
    //    {
    //        var binaryWriterEstimator = new MemoryByteBinaryWriter();
    //        binaryWriterEstimator.Write(Id);
    //        binaryWriterEstimator.Write(ReplyToId);
    //        binaryWriterEstimator.Write(RequireReply);
    //        binaryWriterEstimator.Write(IsEvent);
    //        binaryWriterEstimator.Write(IsAck);
    //        binaryWriterEstimator.WriteNullable(BoardKey);
    //        binaryWriterEstimator.Write(ReplyToBoxId);
    //        binaryWriterEstimator.Write(CorrelationId);
    //        binaryWriterEstimator.WriteNullable(PayloadType);
    //        if (Payload != null)
    //        {
    //            binaryWriterEstimator.WriteRemainingBuffer(Payload.Value);
    //        }

    //        var buffer = writer.GetMemory(4 + binaryWriterEstimator.Offset);
    //        var binaryWriter = new MemoryByteBinaryWriter(buffer);
    //        binaryWriter.Write(binaryWriterEstimator.Offset);
    //        binaryWriter.Write(Id);
    //        binaryWriter.Write(ReplyToId);
    //        binaryWriter.Write(RequireReply);
    //        binaryWriter.Write(IsEvent);
    //        binaryWriter.Write(IsAck);
    //        binaryWriter.WriteNullable(BoardKey);
    //        binaryWriter.Write(ReplyToBoxId);
    //        binaryWriter.Write(CorrelationId);
    //        binaryWriter.WriteNullable(PayloadType);
    //        if (Payload != null)
    //        {
    //            binaryWriter.WriteRemainingBuffer(Payload.Value);
    //        }

    //        writer.Advance(binaryWriter.Offset);
    //    }
    //}
}
