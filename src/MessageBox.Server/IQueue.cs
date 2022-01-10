namespace MessageBox
{
    public interface IQueue : IMessageSource, IMessageSink
    {
        Guid Id { get; }

        string Name { get; }

        void Stop();
    }
}