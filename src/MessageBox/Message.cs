using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox
{
    [MessagePackObject]
    public class Message
    {
        /// <summary>
        /// Unique Id for the message
        /// </summary>
        [Key(0)]
        public Guid Id { get; set; }

        /// <summary>
        /// Id of the message whose this message is a reply
        /// </summary>
        [Key(1)]
        public Guid? ReplyToId { get; set; }

        /// <summary>
        /// Optional Id of the destination box
        /// </summary>
        [Key(2)] 
        public Guid? DestinationBoxId { get; set; }

        /// <summary>
        /// Unique Key of the board where this message is published
        /// </summary>
        [Key(3)] 
        public string? BoardKey { get; set; }

        /// <summary>
        /// Unique Id of the box where the reply to this message should be send
        /// </summary>
        [Key(4)] 
        public Guid? ReplyToBoxId { get; set; }

        /// <summary>
        /// Optional correlation id of the message, useful to "correlate" different messages
        /// </summary>
        [Key(5)] 
        public Guid? CorrelationId { get; set; }

        /// <summary>
        /// Optional payload data
        /// </summary>
        [Key(6)] 
        public byte[]? Payload { get; set; }
    }

    public enum BoardPublishStrategy
    {
        SendToAllSubscribers,

        SendToFirstAvailableInRoundRobinStrategy
    }
}
