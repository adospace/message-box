namespace MessageBox
{
    public interface ITransport
    {
        IMessageSource Source { get; }

        IMessageSink Sink { get; }

        void Stop();

        void Start();
    }
}