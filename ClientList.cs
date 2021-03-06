﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using Serilog;

namespace yacsmu
{
    internal class ClientList
    {

        internal Dictionary<Socket,Client> Collection { get; private set; }
        internal int Count { get => Collection.Count; }

        internal List<Client> GetClientList()
        {
            return Collection.Values.ToList();
        }

        internal ClientList()
        {
            Collection = new Dictionary<Socket, Client>();
        }

        internal void AddClient(Socket socket, Client client)
        {
            Collection.Add(socket, client);
            client.AssignStream(socket);
            Log.Information("CONNECTION: From {remoteEndpoint} at {newClientConnectTime} UTC.", client.RemoteEnd, client.ConnectedAt);
        }

        internal Client GetClientBySocket(Socket socket)
        {
            if (!Collection.TryGetValue(socket, out Client client))
                client = null;

            return client;
        }

        internal Socket GetSocketByClient(Client client)
        {
            Socket socket = Collection.FirstOrDefault(x => x.Value.Id == client.Id).Key;

            return socket;
        }

        internal Client GetClientByID(uint id)
        {
            Client client = Collection.FirstOrDefault(x => x.Value.Id == id).Value;

            return client;
        }

        internal int GetCount()
        {
            return Collection.Count;
        }

        internal void RemoveClient(Client client)
        {
            Collection.Remove(GetSocketByClient(client));
        }

        internal void RemoveClient(Socket socket)
        {
            Collection.Remove(socket);
        }

        private void Kick(Socket socket, Client client)
        {
            socket.Shutdown(SocketShutdown.Both);
            Log.Information("DISCONNECTED: {remoteEndpoint} at {clientDisconnectTime} UTC. Connected for: {clientSessionDuration}.",
                (IPEndPoint)socket.RemoteEndPoint, DateTime.UtcNow, client.SessionDuration);
            client.CloseStream(false);
            socket.Close();
            Collection.Remove(socket);
        }

        internal void KickClient(Client client)
        {
            Socket socket = GetSocketByClient(client);
            Kick(socket, client);
        }

        internal void KickClient(Socket socket)
        {
            Client client = GetClientBySocket(socket);
            Kick(socket, client);
        }

        internal void RemoveInvalidClients()
        {
            foreach (var client in Collection.Where(kv => kv.Value.Status == ClientStatus.Invalid).ToList())
            {
                Kick(client.Key, client.Value);
            }
        }

        internal List<KeyValuePair<Socket,Client>> GetActiveClients()
        {
            // Status > 0 indicates clients that are SUPPOSED to be connected in some way
            return Collection.Where(kv => kv.Value.Status > 0).ToList();
        }

        internal void SendToAll(string message)
        {
            if (Collection.Count > 0)
            {
                foreach (var client in GetClientList())
                {
                    client.Send(message);
                }
            }
        }

        internal void SendTo(List<Client> recipients, string message)
        {
            foreach (var recipient in recipients)
            {
                recipient.Send(message);
            }
        }

        internal void SendToAllExcept(string message, Client excepted_client)
        {

            if (Collection.Count > 0)
            {
                foreach (var client in GetClientList())
                {
                    if (client != excepted_client)
                    {
                        client.Send(message);
                    }
                }
            }
        }

        internal void SendToAllExcept(string message, List<Client> clientList)
        {
            if (Collection.Count > 0)
            {
                foreach (var client in GetClientList())
                {
                    if (!clientList.Contains(client))
                    {
                        client.Send(message);
                    }
                }
            }
        }

        internal void GetAllInput()
        {
            if (Collection.Count >= 0)
            {
                foreach (var client in GetActiveClients())
                {
                    client.Value.ReadInput();
                }
            }
            Commands.ParseInputs();
        }

        internal void FlushAll()
        {
            if (Collection.Count >= 0)
            {
                foreach (var client in GetClientList())
                {
                    if (client.outputBuilder.Length > 0)
                    {
                        client.Flush();
                    }
                    if (client.Status == ClientStatus.Disconnecting && client.outputBuilder.Length == 0)
                    {
                        KickClient(client);
                    }
                }
            }
        }







    }
}
