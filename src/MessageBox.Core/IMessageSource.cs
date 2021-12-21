using MessageBox.Messages;

namespace MessageBox
{
    public interface IMessageSource
    {
        Task<IMessage> GetNextMessageToSend(CancellationToken cancellationToken = default);
    }
}
