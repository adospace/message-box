using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Messages.Implementation
{
    internal enum MessageType
    {
        CallMessage = 1,

        PublishEventMessage = 2,

        ReplyMessage = 3,

        ReplyWithPayloadMessage = 4,

        SetQueueNameMessage = 5,

        SubsribeMessage = 6,

        CallQueuedMessage = 7,
        
        KeepAliveMessage = 8
    }
}
