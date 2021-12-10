using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox
{
    public interface IMessageSource
    {
        Task<Message> GetNextMessageToSend(CancellationToken cancellationToken = default);
    }
}
