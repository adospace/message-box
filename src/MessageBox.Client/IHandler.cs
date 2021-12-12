using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox
{
    public interface IHandler
    { }

    public interface IHandler<T> : IHandler where T : class
    {
        Task Handle(IMessageContext<T> messageContext, CancellationToken cancellationToken = default);
    }

    public interface IHandler<T, RType> : IHandler where T : class
    {
        Task<RType> Handle(IMessageContext<T> messageContext, CancellationToken cancellationToken = default);
    }
}
