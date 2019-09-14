using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace yacsmu
{
    internal enum ClientStatus
    {
        Disconnecting = 0,
        Unauthenticated = 1,
        Authenticating = 2,
        Authenticated = 3,

    }
    
    internal class Client
    {
        uint clientId;
        IPEndPoint clientAddr;
        DateTime dateTimeConnected;

        internal ClientStatus Status { get; set; }

        public Client(uint id, IPEndPoint endpoint)
        {
            clientId = id;
            clientAddr = endpoint;
            dateTimeConnected = DateTime.UtcNow;
            Status = ClientStatus.Unauthenticated;
        }

        internal uint GetID()
        {
            return clientId;
        }
        internal IPEndPoint GetClientAddr()
        {
            return clientAddr;
        }
        internal DateTime GetClientConnectedAt()
        {
            return dateTimeConnected;
        }
        
    }
}
