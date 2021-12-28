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
        ISubscribeMessage CreateSubsribeMessage(string exchangeName);

        ISubscribeQueuedMessage CreateSubsribeQueuedMessage(ISubscribeMessage message, Guid queueId);

        IPublishEventMessage CreatePublishEventMessage(string exchangeName, int timeToLiveSeconds, string payloadType, ReadOnlyMemory<byte> payload);

        ICallMessage CreateCallMessage(string exchangeName, int timeToLiveSeconds, string payloadType, ReadOnlyMemory<byte> payload);

        ICallQueuedMessage CreateCallQueuedMessage(ICallMessage callMessage, Guid queueId);

        IReplyMessage CreateReplyMessage(ICallQueuedMessage message);

        IReplyWithPayloadMessage CreateReplyWithPayloadMessage(ICallQueuedMessage message, string payloadType, ReadOnlyMemory<byte> payload);

        ISetQueueNameMessage CreateSetQueueNameMessage(string queueName);

        ISetQueueNameQueuedMessage CreateSetQueueNameQueuedMessage(string queueName, Guid queueId);

        bool TryDeserialize(ref ReadOnlySequence<byte> buffer, out IMessage? message);
    }
}
