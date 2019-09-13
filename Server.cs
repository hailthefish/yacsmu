using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace yacsmu
{
    internal static class Server
    {
        internal static void SetupServer()
        {
            try
            {
                int port = int.Parse(Config.configuration["Server:port"]);
                var server = new TcpListener(IPAddress.Any, port);
            }
            catch (Exception)
            {
                throw;
            }
            
        }
    }
}
