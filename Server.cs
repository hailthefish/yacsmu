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
            foreach (var client in connectedClients)
            {
                Socket clientSocket = client.Key;
                clientSocket.Disconnect(false);
                clientSocket.Close();
            }
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

        private Client GetClientByID(uint id)
        {
            Client client = connectedClients.FirstOrDefault(x => x.Value.GetID() == id).Value;

            return client;
        }


        internal void CheckIncoming()
        {
            serverSocket.BeginAccept(new AsyncCallback(HandleIncoming), serverSocket);
        }

        internal void CheckAlive()
        {
            var activeClients = connectedClients.Where(kv => kv.Value.Status > 0).ToList();
            foreach (var item in activeClients)
            {
                if (!IsConnected(item.Key))
                {
                    Console.WriteLine(string.Format("DISCONNECTED: {0} at {1}", (IPEndPoint)item.Key.RemoteEndPoint, DateTime.Now));
                    connectedClients.Remove(item.Key);
                }
            }
        }

        private void RemoveInvalidClients()
        {
            foreach (var item in connectedClients.Where(kv => kv.Value.Status == ClientStatus.Invalid).ToList())
            {
                connectedClients.Remove(item.Key);
            }
        }
        

        private bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
        }

        private void HandleIncoming(IAsyncResult ar)
        {
            try
            {
                Socket oldSocket = (Socket)ar.AsyncState;
                Socket newSocket = oldSocket.EndAccept(ar);

                Client newClient = new Client((uint)connectedClients.Count + 1, (IPEndPoint)newSocket.RemoteEndPoint);
                connectedClients.Add(newSocket, newClient);
                Console.WriteLine(string.Format("CONNECTION: From {0} at {1}", (IPEndPoint)newSocket.RemoteEndPoint, DateTime.UtcNow));
            }
            catch
            {
               
            }
            CheckIncoming();
        }



    }
}
