namespace MessageBox
{
    public interface IExchange : IMessageSink
    {
        string Key { get; }

        void Subscribe(IQueue queue);

        void Start();

        void Stop();
    }
}
