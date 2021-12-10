using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Server
{
    public interface IBusServer : IBus
    {
        IBox GetBox(Guid id);

        IBoard GetBoard(string key);
    }
}
