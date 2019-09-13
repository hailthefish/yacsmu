using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace yacsmu
{
    internal class Server
    {
        private readonly IPAddress CONN_IP = IPAddress.Parse("127.0.0.1");
        
        internal int Port { get; private set; }
        internal bool WillAcceptConnections { get; set; }
        internal Dictionary<Socket, ClientConnection> connectedClients;

        private Socket serverSocket;
        
        internal Server()
        {
            connectedClients = new Dictionary<Socket, ClientConnection>();
            WillAcceptConnections = false;

            try
            {
                Port = int.Parse(Config.configuration["Server:port"]);
            }
            catch (Exception)
            {
                throw;
            }
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        internal void Start()
        {
            serverSocket.Bind(new IPEndPoint(CONN_IP, Port));
            Console.WriteLine("Starting server on port " + Port);
            serverSocket.Listen(0);
        }

        internal void Stop()
        {
            Console.WriteLine("Stopping server.");
            serverSocket.Close();
        }

      




        //Telnet Negotiation
        private const byte IAC = 0xFF;
        private const byte DO = 0xFD;
        private const byte DONT = 0xFE;
        private const byte WILL = 0xFB;
        private const byte WONT = 0xFC;
        private const byte NOP = 0xF1;
        //Subnegotiation
        private const byte SB = 0xFA;
        private const byte SE = 0xF0;
        private const byte IS = 0x00;
        private const byte SEND = 0x01;
        //Options
        private const byte SGA = 0x03; // Suppress Go-ahead
        private const byte RFC = 0x21; // Remote Flow Control
        private const byte NAWS = 0x1F; // Negotiate about window size
        private const byte ECHO = 0x01;
        private const byte TYPE = 0x18; // Terminal type
        //Misc
        private const byte GA = 0xF9; //Telnet go-ahead
    }
}
