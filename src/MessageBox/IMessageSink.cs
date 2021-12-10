using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox
{
    public interface IMessageSink
    {
        Task OnReceivedMessage(Message message, CancellationToken cancellationToken = default);
    }
}
