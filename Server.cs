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
        
        internal ClientList clients;

        private Socket serverSocket;

        
        internal Server()
        {
            clients = new ClientList();
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
            try
            {
                foreach (var client in clients.Collection)
                {
                    client.Value.CloseStream(false);
                    Socket clientSocket = client.Key;
                    clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket.BeginDisconnect(false, new AsyncCallback(HandleDisconnect), serverSocket);
                }
                serverSocket.Close();
            }
            catch
            {

                throw;
            }

        }

        
        internal void CheckIncoming()
        {
            if (IsAccepting)
            {
                serverSocket.BeginAccept(new AsyncCallback(HandleIncoming), serverSocket);
            }
            
        }

        internal void CheckAlive()
        {
            if (clients.Count>0)
            {
                foreach (var client in clients.GetActiveClients())
                {
                    if (!IsConnected(client.Key))
                    {
                        client.Value.Status = ClientStatus.Invalid;
                    }

                }
                clients.RemoveInvalidClients();
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
            if (IsAccepting)
            {
                try
                {
                    Socket oldSocket = (Socket)ar.AsyncState;
                    Socket newSocket = oldSocket.EndAccept(ar);
                    IPEndPoint remoteEnd = (IPEndPoint)newSocket.RemoteEndPoint;

                    Client newClient = new Client((uint)clients.Count + 1, remoteEnd);
                    clients.AddClient(newSocket, newClient);
                    Console.WriteLine(string.Format("CONNECTION: From {0} at {1}", remoteEnd, newClient.ConnectedAt));

                    //DirectRawSend(newSocket, new byte[] {Def.IAC,Def.DO,Def.TTYPE }, SocketFlags.None);

                    newClient.Send("¢£¤¦§¨©ª«¬­®¯°±²³´µ·¶¸¹º»¼½¾¿×æ÷ø");
                }
                catch
                {
                    throw;
                }
                CheckIncoming();
            }

        }

        private void HandleDisconnect(IAsyncResult ar)
        {
            ((Socket)ar.AsyncState).Close();
        }


        private void DirectRawSend(Socket socket, byte[] message, SocketFlags socketFlag)
        {
            try
            {
                socket.Send(message, socketFlag);
            }
            catch 
            {
                throw;
            }
            
        }

    }
}
