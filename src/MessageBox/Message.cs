using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace MessageBox
{
    public record Message(
        /// <summary>
        /// Unique Id for the message
        /// </summary>
        Guid Id,

        /// <summary>
        /// Id of the message whose this message is a reply to
        /// </summary>
        Guid? ReplyToId = null,

        /// Indicates if this message require a replay (is the first message in a RPC call)
        bool RequireReply = false,

        //Indicates if this is an event that should be broacasted
        bool IsEvent = false,

        // Indicates if this message is an ack to the message with this Id
        bool IsAck = false,

        /// <summary>
        /// Unique Key of the board where this message is published to
        /// </summary>
        string? BoardKey = null,

        /// <summary>
        /// Unique Id of the box where the reply to this message should be send
        /// </summary>
        Guid? ReplyToBoxId = null,

        /// <summary>
        /// Optional correlation id of the message, useful to "correlate" different messages
        /// </summary>
        Guid? CorrelationId = null,

        /// <summary>
        /// Type name of the object serialized in the Payload property
        /// </summary>
        string? PayloadType = null,

        /// <summary>
        /// Optional payload data
        /// </summary>
        ReadOnlyMemory<byte>? Payload = null,

        IMemoryOwner<byte>? MessageMemoryOwner = null
    )
    {
        public static bool TryDeserialize(ref ReadOnlySequence<byte> buffer, out Message? message)
        {
            message = null;

            if (buffer.Length < 4)
            {
                return false;
            }

            var messageLength = MemoryMarshal.Read<int>(buffer.Slice(0, 4).ToArray());

            if (buffer.Length < 4 + messageLength)
            {
                return false;
            }

            var memoryOwner = MemoryPool<byte>.Shared.Rent(messageLength);

            buffer.Slice(4, messageLength).CopyTo(memoryOwner.Memory.Span);

            var reader = new MemoryByteBinaryReader(memoryOwner.Memory);

            message = new Message(
                Id: reader.ReadGuid(),
                ReplyToId: reader.ReadNullableGuid(),
                RequireReply: reader.ReadBoolean(),
                IsEvent: reader.ReadBoolean(),
                IsAck: reader.ReadBoolean(),
                BoardKey: reader.ReadNullableString(),
                ReplyToBoxId: reader.ReadNullableGuid(),
                CorrelationId: reader.ReadNullableGuid(),
                PayloadType: reader.ReadNullableString(),
                Payload: reader.ReadRemainingBuffer(messageLength),
                MessageMemoryOwner: memoryOwner);

            buffer = buffer.Slice(4 + messageLength);

            return message != null;
        }

        public void Serialize(IBufferWriter<byte> writer)
        {
            var binaryWriterEstimator = new MemoryByteBinaryWriter();
            binaryWriterEstimator.Write(Id);
            binaryWriterEstimator.Write(ReplyToId);
            binaryWriterEstimator.Write(RequireReply);
            binaryWriterEstimator.Write(IsEvent);
            binaryWriterEstimator.Write(IsAck);
            binaryWriterEstimator.WriteNullable(BoardKey);
            binaryWriterEstimator.Write(ReplyToBoxId);
            binaryWriterEstimator.Write(CorrelationId);
            binaryWriterEstimator.WriteNullable(PayloadType);
            if (Payload != null)
            {
                binaryWriterEstimator.WriteRemainingBuffer(Payload.Value);
            }

            var buffer = writer.GetMemory(4 + binaryWriterEstimator.Offset);
            var binaryWriter = new MemoryByteBinaryWriter(buffer);
            binaryWriter.Write(binaryWriterEstimator.Offset);
            binaryWriter.Write(Id);
            binaryWriter.Write(ReplyToId);
            binaryWriter.Write(RequireReply);
            binaryWriter.Write(IsEvent);
            binaryWriter.Write(IsAck);
            binaryWriter.WriteNullable(BoardKey);
            binaryWriter.Write(ReplyToBoxId);
            binaryWriter.Write(CorrelationId);
            binaryWriter.WriteNullable(PayloadType);
            if (Payload != null)
            {
                binaryWriter.WriteRemainingBuffer(Payload.Value);
            }

            writer.Advance(binaryWriter.Offset);
        }
    }

    internal class MemoryByteBinaryReader
    {
        private int _offset;
        public MemoryByteBinaryReader(Memory<byte> memory)
        {
            Memory = memory;
        }

        public Memory<byte> Memory { get; }

        public Guid ReadGuid()
        {
            var guid = new Guid(Memory.Slice(_offset, 16).Span);
            _offset += 16;
            return guid;
        }

        public Guid? ReadNullableGuid()
        {
            var isNull = Memory.Slice(_offset, 1).Span[0] == 0;
            _offset++;
            if (isNull)
            {
                return null;
            }

            var guid = new Guid(Memory.Slice(_offset, 16).Span);
            _offset += 16;
            return guid;
        }

        public bool ReadBoolean()
        {
            var isTrue = Memory.Slice(_offset, 1).Span[0] == 1;
            _offset++;
            return isTrue;
        }

        public string? ReadNullableString()
        {
            var isNull = Memory.Slice(_offset, 1).Span[0] == 0;
            _offset++;
            if (isNull)
            {
                return null;
            }

            var sLen = MemoryMarshal.Read<int>(Memory.Slice(_offset, 4).Span);
            _offset += 4;

            var s = Encoding.Default.GetString(Memory.Slice(_offset, sLen).Span);
            _offset += sLen;

            return s;
        }

        public ReadOnlyMemory<byte>? ReadRemainingBuffer(int messsageLen)
        {
            if (_offset < messsageLen)
            {
                return Memory[_offset..messsageLen];
            }

            return null;
        }
    }

    internal class MemoryByteBinaryWriter
    {
        private readonly Memory<byte>? _memory;

        private int _offset;

        public MemoryByteBinaryWriter(Memory<byte>? memory = null)
        {
            _memory = memory;
        }

        public int Offset => _offset;

        public void Write(int value)
        {
            if (_memory != null)
            {
                MemoryMarshal.Write(_memory.Value.Slice(_offset, 4).Span, ref value);
            }

            _offset += 4;
        }

        public void Write(Guid guid)
        {
            if (_memory != null)
            {
                guid.TryWriteBytes(_memory.Value.Slice(_offset, 16).Span);
            }
            
            _offset += 16;
        }
       
        public void Write(Guid? guid)
        {
            if (_memory != null)
            {
                _memory.Value.Slice(_offset, 1).Span[0] = guid != null ? (byte)1 : (byte)0;
            }
            
            _offset++;

            if (guid == null)
            {
                return;
            }

            if (_memory != null)
            {
                guid.Value.TryWriteBytes(_memory.Value.Slice(_offset, 16).Span);
            }

            _offset += 16;
        }

        public void Write(bool boolValue)
        {
            if (_memory != null)
            {
                _memory.Value.Slice(_offset, 1).Span[0] = boolValue ? (byte)1 : (byte)0;
            }

            _offset++;
        }

        public void WriteNullable(string? stringValue)
        {
            if (_memory != null)
            {
                _memory.Value.Slice(_offset, 1).Span[0] = stringValue != null ? (byte)1 : (byte)0;
            }

            _offset++;

            if (stringValue == null)
            {
                return;
            }

            var sLen = Encoding.Default.GetByteCount(stringValue);
            if (_memory != null)
            {
                MemoryMarshal.Write(_memory.Value.Slice(_offset, 4).Span, ref sLen);
            }

            _offset += 4;

            if (_memory != null)
            {
                Encoding.Default.GetBytes(stringValue, _memory.Value.Slice(_offset, sLen).Span);
            }

            _offset += sLen;
        }

        public void WriteRemainingBuffer(ReadOnlyMemory<byte> buffer)
        {
            if (_memory != null)
            {
                buffer.CopyTo(_memory.Value[_offset..]);
            }

            _offset += buffer.Length;
        }
    }
}
