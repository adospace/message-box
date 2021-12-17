namespace MessageBox
{
    public interface IMessageSink
    {
        Task OnReceivedMessage(Message message, CancellationToken cancellationToken = default);
    }
}
