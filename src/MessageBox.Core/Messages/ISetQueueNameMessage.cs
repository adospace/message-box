using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Messages
{
    public interface ISetQueueNameMessage : IControlMessage
    {
        string SetQueueName { get; }
    }
}
