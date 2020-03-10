using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Serilog;

namespace yacsmu
{

    internal class Server
    {
        internal static readonly IPAddress CONN_IP = IPAddress.Parse("127.0.0.1");
        
        internal string Host { get; private set; }
        internal int Port { get; private set; }
        internal bool IsAccepting { get; private set; }
        internal string Uptime{ get => DateTime.UtcNow.Subtract(StartTime).ToString(@"d\d\ hh\:mm\:ss");}
        internal TimeSpan UptimeSpan { get =>  DateTime.UtcNow.Subtract(StartTime);}
        internal DateTime StartTime { get; private set; }
        internal DateTime ShutdownTime { get; private set; }
        
        internal ClientList clients;

        private Socket serverSocket;

        public event EventHandler<NewClientEventArgs> OnNewClientConnected;

        
        internal Server()
        {
            clients = new ClientList();

            IsAccepting = false;
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                Port = int.Parse(Config.configuration["Server:Port"]);
            }
            catch (Exception e)
            {
                Log.Error("Exception thrown during server start: {exception}", e);
                throw;
            }
        }
        
        internal void Start()
        {
            StartTime = DateTime.UtcNow;
            Log.Information("Starting server on port {Port} at {StartTime} UTC.", Port, StartTime);
            serverSocket.Bind(new IPEndPoint(CONN_IP, Port));
            serverSocket.Listen(0);
            Host = Dns.GetHostName();
            IsAccepting = true;
            CheckIncoming();
        }
        
        internal void Stop()
        {
            IsAccepting = false;
            ShutdownTime = DateTime.UtcNow;
            Log.Information("Stopping server at {ShutdownTime} UTC. Uptime: {Uptime}.", ShutdownTime, Uptime);
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
            catch (Exception e)
            {
                Log.Error("Exception thrown during server stop: {exception}", e);
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

        internal void CheckConnectionsAlive()
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
            catch (SocketException)
            {
                Log.Debug("Connection polling failed for {remoteEndpoint}.");
                return false;
            }
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
                    Log.Verbose("New incoming connection from {remoteEndpoint} at {newClientConnectedAt} UTC.", remoteEnd, DateTime.UtcNow);

                    uint newId;
                    int attemptCount = 0;
                    do
                    {
                        newId = (uint)Program.random.Next(int.MinValue, int.MaxValue);
                        attemptCount++;
                    }
                    while (clients.GetClientByID(newId) != null && attemptCount <= Def.MAX_ATTEMPTS);
                    if (attemptCount <= Def.MAX_ATTEMPTS)
                    {
                        Client newClient = new Client(newId, remoteEnd);
                        clients.AddClient(newSocket, newClient);

                        newClient.SendFile((Config.configuration["Server:TitlescreenPath"]));
                        OnNewClientConnected?.Invoke(this, new NewClientEventArgs(newId, newSocket, newClient));

                        // TODO: Build a negotiation class to handle telnet negotiations per-client
                        //DirectRawSend(newSocket, new byte[] {Def.IAC,Def.DO,Def.TTYPE }, SocketFlags.None);
                    }
                    else
                    {
                        DirectRawSend(newSocket, Encoding.ASCII.GetBytes("Too Many Connected Clients. Try again later."),SocketFlags.None);
                        newSocket.Close();
                    }

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

    internal class NewClientEventArgs : EventArgs
    {
        internal uint Id { get; }
        internal Client Client { get; }
        internal Socket Socket { get; }

        internal NewClientEventArgs(uint _id, Socket _socket, Client _client)
        {
            Id = _id;
            Socket = _socket;
            Client = _client;
        }
    }
}
