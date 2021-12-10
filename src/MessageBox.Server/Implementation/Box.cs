using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MessageBox.Server.Implementation
{
    internal class Box : IBox
    {
        private readonly Channel<Message> _outgoingMessages = Channel.CreateUnbounded<Message>();

        private readonly ConcurrentDictionary<Guid, Message> _messagesToAck = new();
        
        private readonly IMessageSink _messageSink;

        public Box(IMessageSink bus, Guid id)
        {
            _messageSink = bus;
            Id = id;
        }

        public Guid Id { get; }

        public async Task OnReceivedMessage(Message message, CancellationToken cancellationToken)
        { 
            await _outgoingMessages.Writer.WriteAsync(message, cancellationToken);
        }

        public async Task<Message> GetNextMessageToSend(CancellationToken cancellationToken)
        {
            var message = await _outgoingMessages.Reader.ReadAsync(cancellationToken);

            _messagesToAck.TryAdd(message.Id, message);

            return message;
        }

        public void AckMessage(Guid messageId)
            => _messagesToAck.TryRemove(messageId, out var _);

        public async Task DiscardAllPendingMessagesToAck(CancellationToken cancellationToken)
        {
            Message message;
            while ((message = _messagesToAck.FirstOrDefault().Value) != null)
            {
                if (!_messagesToAck.TryRemove(message.Id, out _))
                    continue;

                if (message.BoardKey != null)
                {
                    await _messageSink.OnReceivedMessage(message, cancellationToken);
                }
                else
                {
                    await _outgoingMessages.Writer.WriteAsync(message, cancellationToken);
                }
            }
        }
    }
}
