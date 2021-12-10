using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Client
{
    public interface IMessageSerializer
    {
        byte[] Serialize(object model);

        object Deserialize(byte[] message);
    }
}
