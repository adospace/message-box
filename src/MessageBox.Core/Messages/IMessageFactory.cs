using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Messages
{
    public interface IMessageFactory
    {
        ISubscribeToExchangeMessage CreateSubsribeMessage(string exchangeName);

        IPublishEventMessage CreatePublishEventMessage(string exchangeName, string payloadType, ReadOnlyMemory<byte> payload);

        ICallMessage CreateCallMessage(string exchangeName, bool requireReply, string payloadType, ReadOnlyMemory<byte> payload);

        IReplyMessage CreateReplyMessage(ICallMessage message);

        IReplyWithPayloadMessage CreateReplyWithPayloadMessage(ICallMessage message, string payloadType, ReadOnlyMemory<byte> payload);

        ISetQueueNameMessage CreateSetQueueNameMessage(string queueName);

        bool TryDeserialize(ref ReadOnlySequence<byte> buffer, out IMessage? message);
    }
}
