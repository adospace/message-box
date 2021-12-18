namespace MessageBox.Messages.Implementation
{
    internal class ReplyWithPayloadMessage : ReplyMessage, IReplyWithPayloadMessage
    {
        public ReplyWithPayloadMessage(ICallMessage message, string payloadType, ReadOnlyMemory<byte> payload) 
            : base(message)
        {
            PayloadType = payloadType;
            Payload = payload;
        }

        public string PayloadType { get; }

        public ReadOnlyMemory<byte> Payload { get; }
    }
}