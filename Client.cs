using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace yacsmu
{
    internal enum ClientStatus
    {
        Invalid = -1,
        Disconnecting = 0,
        Unauthenticated = 1,
        Authenticating = 2,
        Authenticated = 3,

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
