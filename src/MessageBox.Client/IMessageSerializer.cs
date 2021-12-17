namespace MessageBox.Client
{
    public interface IMessageSerializer
    {
        byte[] Serialize(object model);

        object Deserialize(ReadOnlyMemory<byte> message, Type targetType);
    }
}
