using System.Runtime.InteropServices;
using System.Text;

namespace MessageBox.Messages.Implementation
{
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
