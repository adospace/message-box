using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Server
{
    public interface IBusServer
    {
        IBox GetOrCreateBox(Guid id);

        IBoard GetOrCreateBoard(string key);
    }
}
