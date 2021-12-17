
using MessageBox;
using Microsoft.Extensions.Hosting;

using var serverHost = Host.CreateDefaultBuilder()
    //Configure MessageBox Server to accept connection from port 12000
    .AddMessageBoxTcpServer(port: 12000)
    .Build();

serverHost.Run();