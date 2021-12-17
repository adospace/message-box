namespace MessageBox.Client.Implementation
{
    internal class MessageContext<T> : IMessageContext<T>
    {
        public MessageContext(T model)
        {
            Model = model;
        }

        public T Model { get; }
    }
}
