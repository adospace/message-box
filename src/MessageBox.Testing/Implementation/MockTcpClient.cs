using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageBox.Testing.Implementation
{
    internal class MockTcpClient
    {



        internal Task OnReceiveMessageFromServer(Message message, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
