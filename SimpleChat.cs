using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace yacsmu
{
    internal class SimpleChat
    {

        private ClientList clients; // Ref to server's client list
        private Dictionary<Client, string> clientColors;

        internal SimpleChat()
        {
            clientColors = new Dictionary<Client, string>();
            clients = Program.server.clients;
            Console.WriteLine("SimpleChat running.");
        }

        internal void Update()
        {
            // Purge color entries for any disconnected clients
            if (clients.Collection.Count < clientColors.Count)
            {
                foreach (var item in clientColors.Select(kv => kv).ToList())
                {
                    if (!clients.Collection.ContainsValue(item.Key))
                    {
                        clientColors.Remove(item.Key);
                    }
                }
            }

            foreach (var clientPair in clients.Collection)
            {   

                Client client = clientPair.Value;
                // Assign a random color for a newly connected client.
                if (!clientColors.ContainsKey(client))
                {
                    clientColors.Add(client, Color.RandomFG());
                    client.Send(string.Format("Simple Chat running on {0}:{1}", Program.server.Host, Program.server.Port));
                }

                foreach (var item in client.inputList)
                {
                    Console.WriteLine(string.Format("{0}: {1}", client.Id,item.Replace(Def.NEWLINE,"\\r\\n")));
                }

                if (client.inputList.Count > 0)
                {
                    string clientInput = client.inputList[0];
                    
                    client.inputList.RemoveAt(0);

                    if (clientInput.ToLower() == "!who")
                    {
                        StringBuilder messageBuilder = new StringBuilder();
                        messageBuilder.Append(Color.FG.Black + Color.BG.Gray +
                            "                                  Simple Chat                                  " +
                            Color.Style.Reset + Def.NEWLINE);
                        foreach (var item in clientColors)
                        {
                            messageBuilder.Append(string.Format("                  {0}{1}{2}{3}",item.Value,item.Key.RemoteEnd.Address, Color.Style.Reset, Def.NEWLINE));
                        }
                        messageBuilder.Append(Color.FG.Black + Color.BG.Gray +
                            "                                                                               " +
                            Color.Style.Reset + Def.NEWLINE);
                        client.Send(messageBuilder.ToString());
                    }
                    else
                    {
                        string timestampPrefix = string.Format("{0} : {1} says: &X", DateTime.UtcNow, client.RemoteEnd.Address);
                        string selfPrefix = string.Format("^g&WSent!^k  You said: ");
                        string message = string.Format(clientColors[client] + clientInput + "&X");

                        clients.SendToAllExcept(Color.ParseTokens(timestampPrefix + message,true), client);
                        client.Send(Color.ParseTokens(selfPrefix + message,true));
                    }
                    clientInput = null;
                }
                
            }


        }

    }
}
