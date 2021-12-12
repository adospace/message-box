using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox
{
    public record Message(
        /// <summary>
        /// Unique Id for the message
        /// </summary>
        Guid Id,

        /// <summary>
        /// Id of the message whose this message is a reply to
        /// </summary>
        Guid? ReplyToId = null,

        /// Indicates if this message require a replay (is the first message in a RPC call)
        bool RequireReply = false,

        /// <summary>
        /// Unique Key of the board where this message is published to
        /// </summary>
        string? BoardKey = null,

        /// <summary>
        /// Unique Id of the box where the reply to this message should be send
        /// </summary>
        Guid? ReplyToBoxId = null,

        /// <summary>
        /// Optional correlation id of the message, useful to "correlate" different messages
        /// </summary>
        Guid? CorrelationId = null,

        /// <summary>
        /// Type name of the object serialized in the Payload property
        /// </summary>
        string? PayloadType = null,

        /// <summary>
        /// Optional payload data
        /// </summary>
        byte[]? Payload = null
    );
}
