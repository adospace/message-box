using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Client.Implementation
{
    internal class MessageContext<T> : IMessageContext<T>
    {
        public MessageContext(T model, Message message)
        {
            Model = model;
            Message = message;
        }

        public T Model { get; }

        public Message Message { get; }
    }
}
