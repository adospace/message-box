using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Messages
{
    public interface ICallMessage : ITransportMessage
    {
        string ExchangeName { get; }

        /// <summary>
        /// Indicates if a reply for the message is required
        /// </summary>
        bool RequireReply { get; }

        /// <summary>
        /// Type name of the object serialized in the Payload property
        /// </summary>
        string PayloadType { get; }

        /// <summary>
        /// Optional payload data
        /// </summary>
        ReadOnlyMemory<byte> Payload { get; }
    }
}
