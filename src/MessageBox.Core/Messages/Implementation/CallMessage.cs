using System.Buffers;

namespace MessageBox.Messages.Implementation
{
    internal class CallMessage : ICallMessage
    {
        public CallMessage(string exchangeName, bool requireReply, string payloadType, ReadOnlyMemory<byte> payload)
        {
            Id = Guid.NewGuid();
            CorrelationId = Guid.NewGuid();
            Payload = payload;
            PayloadType = payloadType;
            ExchangeName = exchangeName;
            RequireReply = requireReply;
        }

        public bool RequireReply { get; }

        public string PayloadType { get; }

        public ReadOnlyMemory<byte> Payload { get; }

        public Guid Id { get; }

        public Guid CorrelationId { get; }

        public string ExchangeName { get; }

        public void Serialize(IBufferWriter<byte> writer)
        {
            throw new NotImplementedException();
        }
    }
}