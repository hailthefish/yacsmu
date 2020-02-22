using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Serilog;

namespace yacsmu
{
    internal class TxStateObj
    {
        internal int dataLength;
        internal NetworkStream networkStream;
        internal byte[] buffer;

        internal TxStateObj(int length, NetworkStream networkStream)
        {
            dataLength = length;
            this.networkStream = networkStream;
        }

        internal TxStateObj(byte [] buffer, NetworkStream networkStream)
        {
            this.buffer = buffer;
            this.networkStream = networkStream;
            this.dataLength = buffer.Length;
        }
    }


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
        internal IPEndPoint RemoteEnd { get; private set; }
        internal DateTime ConnectedAt { get; private set; }
        internal string SessionDuration { get => DateTime.UtcNow.Subtract(ConnectedAt).ToString(@"d\d\ hh\:mm\:ss"); }
        internal TimeSpan SessionSpan { get => DateTime.UtcNow.Subtract(ConnectedAt); }
        internal ClientStatus Status { get; set; }
        internal bool EnabledANSI { get; private set; }

        internal StringBuilder outputBuilder;
        internal Queue<string> inputQueue;
        private string lastAddedInput = null;

        private NetworkStream networkStream;
        private StringBuilder inputBuilder;

        internal Client(uint id, IPEndPoint endpoint)
        {
            Id = id;
            RemoteEnd = endpoint;
            ConnectedAt = DateTime.UtcNow;
            outputBuilder = new StringBuilder(Def.BUF_SIZE / sizeof(char), Def.MAX_BUFFER);
            inputBuilder = new StringBuilder();
            inputQueue = new Queue<string>();
            Status = ClientStatus.Unauthenticated;
            EnabledANSI = true;
        }

        internal void Send(string message)
        {

            message = Color.ParseTokens(message, EnabledANSI);
            outputBuilder.Append(message+Def.NEWLINE);
        }
        internal void Send(string message, bool newline)
        {
            message = Color.ParseTokens(message, EnabledANSI);
            if (newline)
            {
                outputBuilder.Append(message + Def.NEWLINE);
            }
            else outputBuilder.Append(message);
        }

        internal void SendFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Log.Warning("File not found: {0}", filePath);
                Send("&RFile not found!&X");
            }
            else
            {
                FileInfo file = new FileInfo(filePath);
                if (file.Length > Def.MAX_BUFFER)
                {
                    Log.Warning("File too long: {0}", filePath);
                    Send("&RFile too long!&X");
                }
                else
                {
                    try
                    {
                        Send(File.ReadAllText(filePath));
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }
        }

        internal void Flush()
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
                    networkStream.BeginWrite(sendData, 0, sendData.Length, new AsyncCallback(WriteCallback), new TxStateObj(sendData.Length,networkStream));
                }
                catch
                {

                    throw;
                }
            }
            
        }

        private void WriteCallback(IAsyncResult ar)
        {
            NetworkStream sendingStream = ((TxStateObj)ar.AsyncState).networkStream;
            try
            {
                sendingStream.EndWrite(ar);
            }
            catch
            {

                throw;
            }
            Log.Debug("Sent {sentBytes} bytes to {remoteEndpoint}.", ((TxStateObj)ar.AsyncState).dataLength, RemoteEnd);
        }

        internal void ReadInput()
        {
            if ((networkStream != null) && (networkStream.CanRead) && networkStream.DataAvailable)
            {
                var readBuffer = new byte[Def.BUF_SIZE];
                networkStream.BeginRead(readBuffer,0,Def.BUF_SIZE,new AsyncCallback(ReadCallback), new TxStateObj(readBuffer,networkStream));
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            try
            {
                NetworkStream networkStream = ((TxStateObj)ar.AsyncState).networkStream;
                byte[] buffer = ((TxStateObj)ar.AsyncState).buffer;

                int bytesReceived = networkStream.EndRead(ar);
                string inputReceived = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                Log.Debug("Read {readBytes} bytes from {remoteEndpoint}.",bytesReceived, RemoteEnd);
                inputBuilder.Append(inputReceived);
                ChunkifyInput();

            }
            catch
            {

                throw;
            }
        }

        private void ChunkifyInput()
        {
            using (StringReader reader = new StringReader(inputBuilder.ToString()))
            {
                inputBuilder.Clear();
                string line;
                while ((line = reader.ReadLine())!= null)
                {
                    switch (line)
                    {
                        case "!":
                            if(lastAddedInput != null)inputQueue.Enqueue(lastAddedInput);
                            break;
                        case "!x":
                            inputQueue.Clear();
                            break;
                        default:
                            inputQueue.Enqueue(line);
                            lastAddedInput = line;
                            break;
                    }
                }
                
            }
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
