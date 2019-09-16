using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

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
        internal string SessionDuration { get => DateTime.UtcNow.Subtract(ConnectedAt).ToString(@"d\d\ hh\:mm\:ss"); }
        internal TimeSpan SessionSpan { get => DateTime.UtcNow.Subtract(ConnectedAt); }
        internal ClientStatus Status { get; set; }

        internal StringBuilder outputBuilder;

        private NetworkStream networkStream;

        internal Client(uint id, IPEndPoint endpoint)
        {
            Id = id;
            Endpoint = endpoint;
            ConnectedAt = DateTime.UtcNow;
            outputBuilder = new StringBuilder(Def.BUF_SIZE/sizeof(char),Def.MAX_OUTPUT);
            Status = ClientStatus.Unauthenticated;
        }

        internal void AddOutput(string message)
        {
            outputBuilder.Append(message);
        }

        internal void SendOutput()
        {
            if ((networkStream != null) && (networkStream.CanWrite) && outputBuilder.Length > 0)
            {
                char[] sendChar;
                if (outputBuilder.Length > (Def.BUF_SIZE))
                {
                    sendChar = new char[(Def.BUF_SIZE)];
                }
                else
                {
                    sendChar = new char[outputBuilder.Length];
                    
                }
                outputBuilder.CopyTo(0, sendChar, 0, sendChar.Length);
                outputBuilder.Remove(0,sendChar.Length);
                byte[] sendData = Encoding.ASCII.GetBytes(sendChar);
                try
                {
                    networkStream.BeginWrite(sendData, 0, sendData.Length, SendComplete, sendData);
                }
                catch (Exception)
                {

                    throw;
                }
            }
            
        }

        private void SendComplete(IAsyncResult ar)
        {
            Console.WriteLine("Sent {0} bytes to {1}",((byte[])ar.AsyncState).Length,this.Endpoint);
        }

        internal void AssignStream(Socket socket)
        {
            networkStream = new NetworkStream(socket);
        }
        

        internal void CloseStream(bool waitForTimeout)
        {
            if (networkStream != null)
            {
                if (waitForTimeout) networkStream.Close(Def.STREAM_TIMEOUT);
                else networkStream.Dispose();
            }
        }

    }
}
