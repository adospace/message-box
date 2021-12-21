using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Messages.Implementation
{
    internal class CallQueuedMessage : ICallQueuedMessage
    {
        private bool _disposedValue;
        private IMemoryOwner<byte>? _memoryOwner;

        public CallQueuedMessage(ICallMessage callMessage, Guid sourceQueueId)
        {
            Id = callMessage.Id;
            CorrelationId = callMessage.CorrelationId;
            ExchangeName = callMessage.ExchangeName;
            SourceQueueId = sourceQueueId;
            PayloadType = callMessage.PayloadType;
            Payload = callMessage.Payload;
        }

        public CallQueuedMessage(Guid id, Guid correlationId, string exchangeName, Guid sourceQueueId, string payloadType, ReadOnlyMemory<byte> payload, IMemoryOwner<byte> memoryOwner)
        {
            Id = id;
            CorrelationId = correlationId;
            ExchangeName = exchangeName;
            SourceQueueId = sourceQueueId;
            PayloadType = payloadType;
            Payload = payload;
            _memoryOwner = memoryOwner;
        }


        public Guid SourceQueueId { get; }

        public string ExchangeName { get; }

        public bool RequireReply { get; }

        public string PayloadType { get; }

        public ReadOnlyMemory<byte> Payload { get; }


        public Guid Id { get; }

        public Guid CorrelationId { get; }

        public virtual void Serialize(IBufferWriter<byte> writer)
        {
            var binaryWriterEstimator = new MemoryByteBinaryWriter();
            binaryWriterEstimator.Write(Id);
            binaryWriterEstimator.Write(CorrelationId);
            binaryWriterEstimator.Write(ExchangeName);
            binaryWriterEstimator.Write(SourceQueueId);
            binaryWriterEstimator.Write(PayloadType);
            binaryWriterEstimator.WriteRemainingBuffer(Payload);

            var messageLength = binaryWriterEstimator.Offset;

            var buffer = writer.GetMemory(5 + messageLength);
            var binaryWriter = new MemoryByteBinaryWriter(buffer);
            binaryWriter.Write((byte)MessageType.CallQueuedMessage);
            binaryWriter.Write(messageLength);
            binaryWriter.Write(Id);
            binaryWriter.Write(CorrelationId);
            binaryWriter.Write(ExchangeName);
            binaryWriter.Write(SourceQueueId);
            binaryWriter.Write(PayloadType);
            binaryWriter.WriteRemainingBuffer(Payload);

            writer.Advance(5 + messageLength);
        }

        public static void TryDeserialize(ref ReadOnlySequence<byte> buffer, out IMessage? message)
        {
            message = null;

            if (buffer.Length < 5)
            {
                return;
            }

            var messageLength = MemoryMarshal.Read<int>(buffer.Slice(1, 4).ToArray());

            if (buffer.Length < 5 + messageLength)
            {
                return;
            }

            var memoryOwner = MemoryPool<byte>.Shared.Rent(messageLength);

            buffer.Slice(5, messageLength).CopyTo(memoryOwner.Memory.Span);

            var reader = new MemoryByteBinaryReader(memoryOwner.Memory);
 
            message = new CallQueuedMessage(
                id: reader.ReadGuid(),
                correlationId: reader.ReadGuid(),
                exchangeName: reader.ReadString(),
                sourceQueueId: reader.ReadGuid(),
                payloadType: reader.ReadString(),
                payload: reader.ReadRemainingBuffer(messageLength),
                memoryOwner: memoryOwner);

            buffer = buffer.Slice(5 + messageLength);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _memoryOwner?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CallQueuedMessage()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
