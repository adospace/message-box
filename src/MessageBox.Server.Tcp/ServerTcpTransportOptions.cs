using System.Net;

namespace MessageBox;

public class ServerTcpTransportOptions : TcpTransportOptions, ICleanUpServiceOptions
{
    public ServerTcpTransportOptions(int port) : base(port)
    {
    }

    public ServerTcpTransportOptions(string ipString, int port) : base(ipString, port)
    {
    }

    public ServerTcpTransportOptions(IPAddress address, int port) : base(address, port)
    {
    }

    public ServerTcpTransportOptions(IPEndPoint serverEndPoint) : base(serverEndPoint)
    {
    }

    public TimeSpan KeepAliveTimeout { get; set; } = TimeSpan.FromSeconds(10);
}