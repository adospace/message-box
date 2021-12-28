using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Messages.Implementation
{
    internal class MessageFactory : IMessageFactory
    {
        public ICallMessage CreateCallMessage(string exchangeName, int timeToLiveSeconds, string payloadType, ReadOnlyMemory<byte> payload) 
            => new CallMessage(exchangeName, timeToLiveSeconds, payloadType, payload);

        public ICallQueuedMessage CreateCallQueuedMessage(ICallMessage callMessage, Guid queueId)
            => new CallQueuedMessage(callMessage, queueId);

        public IPublishEventMessage CreatePublishEventMessage(string exchangeName, int timeToLiveSeconds, string payloadType, ReadOnlyMemory<byte> payload) 
            => new PublishEventMessage(exchangeName, timeToLiveSeconds, payloadType, payload);

        public IReplyMessage CreateReplyMessage(ICallQueuedMessage message) 
            => new ReplyMessage(message);

        public IReplyWithPayloadMessage CreateReplyWithPayloadMessage(ICallQueuedMessage message, string payloadType, ReadOnlyMemory<byte> payload) 
            => new ReplyWithPayloadMessage(message, payloadType, payload);

        public ISetQueueNameMessage CreateSetQueueNameMessage(string queueName)
            => new SetQueueNameMessage(queueName);
        
        public ISetQueueNameQueuedMessage CreateSetQueueNameQueuedMessage(string queueName, Guid queueId)
            => new SetQueueNameQueuedMessage(queueName, queueId);

        public ISubscribeMessage CreateSubsribeMessage(string exchangeName)
            => new SubscribeMessage(exchangeName);

        public ISubscribeQueuedMessage CreateSubsribeQueuedMessage(ISubscribeMessage message, Guid queueId)
            => new SubscribeQueuedMessage(message, queueId);
        
        public IKeepAliveMessage CreateKeepAliveMessage(Guid queueId) 
            => new KeepAliveMessage(queueId);

        public bool TryDeserialize(ref ReadOnlySequence<byte> buffer, out IMessage? message)
        {
            message = null;
            if (buffer.Length < 1)
            {
                return false;
            }

            var messageType = (MessageType)MemoryMarshal.Read<byte>(buffer.Slice(0, 1).ToArray());

            switch (messageType)
            {
                case MessageType.CallMessage:
                    CallMessage.TryDeserialize(ref buffer, out message);
                    break;
                case MessageType.CallQueuedMessage:
                    CallQueuedMessage.TryDeserialize(ref buffer, out message);
                    break;
                case MessageType.PublishEventMessage:
                    PublishEventMessage.TryDeserialize(ref buffer, out message);
                    break;
                case MessageType.ReplyMessage:
                    ReplyMessage.TryDeserialize(ref buffer, out message);
                    break;
                case MessageType.ReplyWithPayloadMessage:
                    ReplyWithPayloadMessage.TryDeserialize(ref buffer, out message);
                    break;
                case MessageType.SetQueueNameMessage:
                    SetQueueNameMessage.TryDeserialize(ref buffer, out message);
                    break;
                case MessageType.SubsribeMessage:
                    SubscribeMessage.TryDeserialize(ref buffer, out message);
                    break;
                case MessageType.KeepAliveMessage:
                    KeepAliveMessage.TryDeserialize(ref buffer, out message);
                    break;
                default:
                    throw new NotSupportedException();
            }

            return message != null;

            //        message = null;

            //        if (buffer.Length < 4)
            //        {
            //            return false;
            //        }

            //        var messageLength = MemoryMarshal.Read<int>(buffer.Slice(0, 4).ToArray());

            //        if (buffer.Length < 4 + messageLength)
            //        {
            //            return false;
            //        }

            //        var memoryOwner = MemoryPool<byte>.Shared.Rent(messageLength);

            //        buffer.Slice(4, messageLength).CopyTo(memoryOwner.Memory.Span);

            //        var reader = new MemoryByteBinaryReader(memoryOwner.Memory);

            //        message = new Message(
            //            Id: reader.ReadGuid(),
            //            ReplyToId: reader.ReadNullableGuid(),
            //            RequireReply: reader.ReadBoolean(),
            //            IsEvent: reader.ReadBoolean(),
            //            IsAck: reader.ReadBoolean(),
            //            BoardKey: reader.ReadNullableString(),
            //            ReplyToBoxId: reader.ReadNullableGuid(),
            //            CorrelationId: reader.ReadNullableGuid(),
            //            PayloadType: reader.ReadNullableString(),
            //            Payload: reader.ReadRemainingBuffer(messageLength),
            //            MessageMemoryOwner: memoryOwner);

            //        buffer = buffer.Slice(4 + messageLength);

            //        return message != null;
        }
    }
}
