namespace MessageBox.Client
{
    public interface IMessageSerializerFactory
    {
        IMessageSerializer CreateMessageSerializer();
    }
}
