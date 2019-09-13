using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace yacsmu
{
    internal static class Server
    {
        internal const string LINE_END = "\r\n";

        static TcpListener listener;

        internal static void SetupServer()
        {
            try
            {
                int port = int.Parse(Config.configuration["Server:port"]);
                listener = new TcpListener(IPAddress.Any, port);
            }
            catch (Exception)
            {
                throw;
            }

            listener.Start();
        }

    }
}
