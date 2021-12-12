using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox
{
    public interface IBus
    {
        Task Run(CancellationToken cancellationToken = default);

        Task Stop(CancellationToken cancellationToken = default);
    }
}
