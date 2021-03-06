using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Messages
{
    public interface ICallMessage : ITransportMessage, IDisposable
    {
        string ExchangeName { get; }

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
