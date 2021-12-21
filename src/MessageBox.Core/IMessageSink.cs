using MessageBox.Messages;
using System.Buffers;

namespace MessageBox
{
    public interface IMessageSink
    {
        Task OnReceivedMessage(IMessage message, CancellationToken cancellationToken = default);
    }
}
