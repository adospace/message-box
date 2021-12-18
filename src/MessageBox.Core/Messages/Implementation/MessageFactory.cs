using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Messages.Implementation
{
    internal class MessageFactory : IMessageFactory
    {
        public ICallMessage CreateCallMessage(string exchangeName, bool requireReply, string payloadType, ReadOnlyMemory<byte> payload) 
            => new CallMessage(exchangeName, requireReply, payloadType, payload);

        public IPublishEventMessage CreatePublishEventMessage(string exchangeName, string payloadType, ReadOnlyMemory<byte> payload) 
            => new PublishEventMessage(exchangeName, payloadType, payload);

        public IReplyMessage CreateReplyMessage(ICallMessage message) 
            => new ReplyMessage(message);

        public IReplyWithPayloadMessage CreateReplyWithPayloadMessage(ICallMessage message, string payloadType, ReadOnlyMemory<byte> payload) 
            => new ReplyWithPayloadMessage(message, payloadType, payload);

        public ISetQueueNameMessage CreateSetQueueNameMessage(string queueName)
            => new SetQueueNameMessage(queueName);

        public ISubscribeToExchangeMessage CreateSubsribeMessage(string exchangeName)
            => new SubsribeMessage(exchangeName);

        public bool TryDeserialize(ref ReadOnlySequence<byte> buffer, out IMessage? message)
        {
            throw new NotImplementedException();
        }
    }
}
