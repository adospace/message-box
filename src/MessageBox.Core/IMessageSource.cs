namespace MessageBox
{
    public interface IMessageSource
    {
        Task<Message> GetNextMessageToSend(CancellationToken cancellationToken = default);
    }
}
