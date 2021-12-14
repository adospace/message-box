using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace MessageBox
{
    public class MessageBoxCallException : Exception
    {
        public MessageBoxCallException()
        {
        }

        public MessageBoxCallException(string? message) : base(message)
        {
        }

        public MessageBoxCallException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected MessageBoxCallException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

}
