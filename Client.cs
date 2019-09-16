using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace yacsmu
{
    internal enum ClientStatus
    {
        Invalid = -1, // Client is due to be purged from the collection
        Disconnecting = 0, // Client is going to be disconnecting soon but we want to send it more data
        Unauthenticated = 1, // Connected but not logged in to an account
        Authenticating = 2, // In the process of logging in to an account
        Authenticated = 3, // Logged in to an account
        Playing = 4 // Logged in to a character
    }
    
    internal class Client
    {
        internal uint Id { get; private set; }
        internal IPEndPoint Endpoint { get; private set; }
        internal DateTime ConnectedAt { get; private set; }

        internal ClientStatus Status { get; set; }

        public Client(uint id, IPEndPoint endpoint)
        {
            Id = id;
            Endpoint = endpoint;
            ConnectedAt = DateTime.UtcNow;
            Status = ClientStatus.Unauthenticated;
        }

    }
}
