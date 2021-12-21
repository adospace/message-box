using System.Buffers;

namespace MessageBox.Messages.Implementation
{
    internal class SubsribeQueuedMessage : ISubscribeQueuedMessage
    {
        public SubsribeQueuedMessage(ISubscribeMessage message, Guid sourceQueueId)
        {
            ExchangeName = message.ExchangeName;
            SourceQueueId = sourceQueueId;
        }

        public Guid SourceQueueId { get; }

        public string ExchangeName { get; }
    }
}