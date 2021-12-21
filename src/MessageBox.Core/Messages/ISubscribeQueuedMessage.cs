using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Messages
{
    public interface ISubscribeQueuedMessage : IControlMessage, IQueuedMessage
    {
        string ExchangeName { get; }
    }
}
