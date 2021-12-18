using System.Buffers;

namespace MessageBox.Messages.Implementation
{
    internal class SubsribeMessage : ISubscribeToExchangeMessage
    {
        public SubsribeMessage(string exchangeName)
        {
            ExchangeName = exchangeName;
        }

        public string ExchangeName { get; }

        public void Serialize(IBufferWriter<byte> writer)
        {
            throw new NotImplementedException();
        }
    }
}