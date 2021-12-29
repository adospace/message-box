namespace MessageBox
{
    public interface IQueue : IMessageSource, IMessageSink
    {
        Guid Id { get; }

        void Stop();
    }
}