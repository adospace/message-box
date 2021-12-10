namespace MessageBox
{
    public interface IBox : IMessageSource, IMessageSink
    {
        Guid Id { get; }
    }
}