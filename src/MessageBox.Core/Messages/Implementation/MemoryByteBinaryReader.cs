using System.Runtime.InteropServices;
using System.Text;

namespace MessageBox.Messages.Implementation
{
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
}
