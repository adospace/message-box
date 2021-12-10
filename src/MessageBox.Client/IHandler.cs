using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Client
{
    public interface IHandler<T> : IHandler
    {
        Task Handle(IMessageContext<T> messageContext, CancellationToken cancellationToken = default);
    }

    public interface IHandler
    { 
    }
}
