using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Client.Implementation
{
    internal class MessageContext<T> : IMessageContext<T>
    {
        private readonly Bus _busClient;
        private readonly Message _message;

        public MessageContext(Bus busClient, T model, Message message)
        {
            _busClient = busClient;
            Model = model;
            _message = message;
        }

        public T Model { get; }

        public async Task Reply<R>(R replyModel, CancellationToken cancellationToken = default)
        {
            await _busClient.Reply(_message, replyModel, cancellationToken);
        }
    }
}
