using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Messages
{
    public interface ITransportMessage : IMessage, ISerializableMessage
    {
        Guid Id { get; }

        Guid CorrelationId { get; }

        int TimeToLiveSeconds { get; }
    }
}
