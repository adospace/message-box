using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;

namespace MessageBox.Messages.Implementation
{
    internal class MemoryByteBinaryReader
    {
        private readonly Memory<byte> _memory;
        private int _offset;
        public MemoryByteBinaryReader(Memory<byte> memory, int offset = 0)
        {
            _memory = memory;
            _offset = offset;
        }

        public Guid ReadGuid()
        {
            var guid = new Guid(_memory.Slice(_offset, 16).Span);
            _offset += 16;
            return guid;
        }

        public Guid? ReadNullableGuid()
        {
            var isNull = _memory.Slice(_offset, 1).Span[0] == 0;
            _offset++;
            if (isNull)
            {
                return null;
            }

            var guid = new Guid(_memory.Slice(_offset, 16).Span);
            _offset += 16;
            return guid;
        }

        public bool ReadBoolean()
        {
            var isTrue = _memory.Slice(_offset, 1).Span[0] == 1;
            _offset++;
            return isTrue;
        }

        public string ReadString()
        {
            var sLen = MemoryMarshal.Read<int>(_memory.Slice(_offset, 4).Span);
            _offset += 4;

            return ReadString(sLen);
        }

        public string ReadString(int sLen)
        {
            var s = Encoding.Default.GetString(_memory.Slice(_offset, sLen).Span);
            _offset += sLen;

            return s;
        }

        public string? ReadNullableString()
        {
            var isNull = _memory.Slice(_offset, 1).Span[0] == 0;
            _offset++;
            if (isNull)
            {
                return null;
            }

            var sLen = MemoryMarshal.Read<int>(_memory.Slice(_offset, 4).Span);
            _offset += 4;

            var s = Encoding.Default.GetString(_memory.Slice(_offset, sLen).Span);
            _offset += sLen;

            return s;
        }

        public ReadOnlyMemory<byte> ReadRemainingBuffer(int messsageLen)
        {
            return _memory[_offset..messsageLen];
        }
    }
}
