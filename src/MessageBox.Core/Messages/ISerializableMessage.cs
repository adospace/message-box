using System.Buffers;

namespace MessageBox.Messages
{
    public interface ISerializableMessage : IMessage
    {
        void Serialize(IBufferWriter<byte> writer);
    }
}
