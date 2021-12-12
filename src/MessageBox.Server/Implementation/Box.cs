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

        public Box(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }

        public async Task OnReceivedMessage(Message message, CancellationToken cancellationToken)
        { 
            await _outgoingMessages.Writer.WriteAsync(message, cancellationToken);
        }

        public async Task<Message> GetNextMessageToSend(CancellationToken cancellationToken)
        {
            return await _outgoingMessages.Reader.ReadAsync(cancellationToken);
        }
    }
}
