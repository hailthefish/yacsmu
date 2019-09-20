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
            // Purge color entries for any disconnected clients.
            foreach (var item in clientColors.Select(kv => kv).ToList())
            {
                if (!clients.Collection.ContainsValue(item.Key))
                {
                    clientColors.Remove(item.Key);
                }
            }

            foreach (var clientPair in clients.Collection)
            {   

                Client client = clientPair.Value;
                // Assign a random color for a newly connected client.
                if (!clientColors.ContainsKey(client))
                {
                    clientColors.Add(client, Color.RandomFG());
                    client.Send("SimpleChat running on " + Program.server.Host + ":" + Program.server.Port);
                }

                if (client.inputList.Count > 0)
                {
                    string clientInput = client.inputList[0];
                    
                    client.inputList.RemoveAt(0);

                    if (clientInput == "!who")
                    {
                        StringBuilder messageBuilder = new StringBuilder();
                        messageBuilder.Append(Color.FG.Black + Color.BG.Gray +
                            "                  SimpleChat                  " +
                            Color.Style.Reset + Def.NEWLINE);
                        foreach (var item in clientColors)
                        {
                            messageBuilder.Append(item.Value + item.Key.RemoteEnd.Address + Color.Style.Reset + Def.NEWLINE);
                        }
                        messageBuilder.Append(Color.FG.Black + Color.BG.Gray +
                            "                                              " +
                            Color.Style.Reset + Def.NEWLINE);
                        client.Send(messageBuilder.ToString());
                    }
                    else
                    {
                        string message = Color.FG.White +
                            string.Format("{0} : {1} says: ", DateTime.UtcNow, client.RemoteEnd.Address) +
                            clientColors[client] + Color.ParseTokens(clientInput,true) + Color.Style.Reset;
                        clients.SendToAllExcept(message + Color.Style.Reset, client);
                        client.Send(Color.FG.White + Color.BG.DGreen + "Sent!" + Color.Style.Reset + " : " + message + Color.Style.Reset);
                    }

                }
                
            }


        }

    }
}
