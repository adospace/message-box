using System.Net;

namespace MessageBox
{
    public class TcpTransportOptions
    {
        public IPEndPoint ServerEndPoint { get; }

        public TcpTransportOptions(int port)
        {
            ServerEndPoint = new IPEndPoint(IPAddress.Any, port);
        }

        public TcpTransportOptions(string ipString, int port)
        { 
            ServerEndPoint = new IPEndPoint(IPAddress.Parse(ipString), port);
        }

        public TcpTransportOptions(IPAddress address, int port)
        {
            ServerEndPoint = new IPEndPoint(address, port);
        }

        public TcpTransportOptions(IPEndPoint serverEndPoint)
        {
            ServerEndPoint = serverEndPoint;
        }
    }
}
