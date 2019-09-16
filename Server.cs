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
        internal string Uptime{ get => DateTime.UtcNow.Subtract(StartTime).ToString(@"d\d\ hh\:mm\:ss");}
        internal TimeSpan UptimeSpan { get =>  DateTime.UtcNow.Subtract(StartTime);}
        internal DateTime StartTime { get; private set; }
        internal DateTime ShutdownTime { get; private set; }

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
            StartTime = DateTime.UtcNow;
            Console.WriteLine("Starting server on port {0} at {1}", Port, StartTime);
            serverSocket.Bind(new IPEndPoint(CONN_IP, Port));
            serverSocket.Listen(0);
            IsAccepting = true;
            CheckIncoming();
        }
        
        internal void Stop()
        {
            IsAccepting = false;
            ShutdownTime = DateTime.UtcNow;
            Console.WriteLine("Stopping server at {0}. Uptime: {1}.", ShutdownTime, Uptime);
            Console.WriteLine();
            foreach (var client in connectedClients)
            {
                Socket clientSocket = client.Key;
                clientSocket.Shutdown(SocketShutdown.Both);
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
            Socket socket = connectedClients.FirstOrDefault(x => x.Value.Id == client.Id).Key;

            return socket;
        }

        private Client GetClientByID(uint id)
        {
            Client client = connectedClients.FirstOrDefault(x => x.Value.Id == id).Value;

            return client;
        }


        internal void CheckIncoming()
        {
            serverSocket.BeginAccept(new AsyncCallback(HandleIncoming), serverSocket);
        }

        internal void CheckAlive()
        {
            // Status > 0 indicates clients that are SUPPOSED to be connected in some way
            // New list so that we can remove them without changing the thing we're iterating over
            var activeClients = connectedClients.Where(kv => kv.Value.Status > 0).ToList();
            foreach (var client in activeClients)
            {
                if (!IsConnected(client.Key))
                {
                    client.Key.Shutdown(SocketShutdown.Both);
                    client.Key.Disconnect(false);
                    TimeSpan durationSpan = DateTime.UtcNow.Subtract(client.Value.ConnectedAt);
                    Console.WriteLine(string.Format("DISCONNECTED: {0} at {1}. Connected for {2}.",
                        (IPEndPoint)client.Key.RemoteEndPoint,
                        DateTime.UtcNow,
                        durationSpan.ToString(@"d\d\ hh\:mm\:ss")
                        ));
                    connectedClients.Remove(client.Key);
                    client.Key.Close();
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
