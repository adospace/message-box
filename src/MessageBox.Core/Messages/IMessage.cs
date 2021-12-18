using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Messages
{
    public interface IMessage
    {
        void Serialize(IBufferWriter<byte> writer);
    }
}
