using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Messages
{
    public interface ICallQueuedMessage : ICallMessage, IQueuedMessage
    {
    }
}
