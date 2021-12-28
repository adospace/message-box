using System.Net;

namespace MessageBox
{
    public class TcpBusClientOptions : TcpTransportOptions, IBusClientOptions
    {
        public TcpBusClientOptions(int port) : base(port)
        {
        }

        public TcpBusClientOptions(IPEndPoint serverEndPoint) : base(serverEndPoint)
        {
        }

        public TcpBusClientOptions(string ipString, int port) : base(ipString, port)
        {
        }

        public TcpBusClientOptions(IPAddress address, int port) : base(address, port)
        {
        }

        public string? Name { get; set; }

        public TimeSpan DefaultCallTimeout { get; set; } = TimeSpan.FromSeconds(60);

        public int MaxDegreeOfParallelism { get; set; } = 10;
    }
}
