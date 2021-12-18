using System.Buffers;

namespace MessageBox.Messages.Implementation
{
    internal class PublishEventMessage : IPublishEventMessage
    {
        public PublishEventMessage(string exchangeName, string payloadType, ReadOnlyMemory<byte> payload)
        {
            Id = Guid.NewGuid();
            CorrelationId = Guid.NewGuid();
            ExchangeName = exchangeName;
            PayloadType = payloadType;
            Payload = payload;
        }

        public string ExchangeName { get; }
        public string PayloadType { get; }
        public ReadOnlyMemory<byte> Payload { get; }

        public Guid Id { get; }

        public Guid CorrelationId { get; }

        public void Serialize(IBufferWriter<byte> writer)
        {
            throw new NotImplementedException();
        }
    }
}