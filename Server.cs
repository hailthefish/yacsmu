using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace yacsmu
{
    internal class Server
    {
        private readonly IPAddress CONN_IP = IPAddress.Parse("127.0.0.1");
        
        internal int Port { get; private set; }
        internal bool IsAccepting { get; private set; }

        internal Dictionary<Socket, Client> connectedClients;

        private Socket serverSocket;
        
        internal Server()
        {
            connectedClients = new Dictionary<Socket, Client>();
            IsAccepting = false;
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Port = int.Parse(Config.configuration["Server:port"]);
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        internal void Start()
        {
            Console.WriteLine("Starting server on port " + Port);
            serverSocket.Bind(new IPEndPoint(CONN_IP, Port));
            serverSocket.Listen(0);
            IsAccepting = true;
            CheckIncoming();
        }
        
        internal void Stop()
        {
            IsAccepting = false;
            Console.WriteLine("Stopping server.");
            serverSocket.Close();
        }

        private Client GetClientBySocket(Socket socket)
        {
            if (!connectedClients.TryGetValue(socket, out Client client))
                client = null;

            return client;
        }

        private Socket GetSocketByClient(Client client)
        {
            Socket socket = connectedClients.FirstOrDefault(x => x.Value.GetID() == client.GetID()).Key;

            return socket;
        }


        internal void CheckIncoming()
        {
            serverSocket.BeginAccept(new AsyncCallback(HandleIncoming), serverSocket);
        }

        private void HandleIncoming(IAsyncResult ar)
        {
            try
            {
                Socket oldSocket = (Socket)ar.AsyncState;
                Socket newSocket = oldSocket.EndAccept(ar);

                Client newClient = new Client((uint)connectedClients.Count + 1, (IPEndPoint)newSocket.RemoteEndPoint);
                connectedClients.Add(newSocket, newClient);
                Console.WriteLine(string.Format("CONNECTION: From {0} at {1}", (IPEndPoint)newSocket.RemoteEndPoint, DateTime.Now));
            }
            catch
            {
               
            }
        }



    }
}
