using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

namespace yacsmu
{
    internal class SimpleChat
    {
        private const string prompt = "> ";

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
            List<Client> clientList = clients.GetClientList();
            // Purge color entries for any disconnected clients
            if (clientList.Count < clientColors.Count)
            {
                foreach (var item in clientColors.Select(kv => kv).ToList())
                {
                    if (!clientList.Contains(item.Key))
                    {
                        clientColors.Remove(item.Key);
                    }
                }
            }


            foreach (var client in clientList)
            {   
                // Assign a random color for a newly connected client.
                if (!clientColors.ContainsKey(client))
                {
                    clientColors.Add(client, Color.RandomFG());
                    client.Send(string.Format("Simple Chat running on {0}:{1}", Program.server.Host, Program.server.Port));
                    client.Send(prompt + Def.NEWLINE);
                }
                
                if (client.inputQueue.Count > 0)
                {
                    string clientInput = client.inputQueue.Dequeue();

                    if (clientInput.ToLower() == "!who")
                    {
                        StringBuilder messageBuilder = new StringBuilder();
                        messageBuilder.Append(Color.FG.Black + Color.BG.Gray +
                            "                                  Simple Chat                                  " +
                            Color.Style.Reset + Def.NEWLINE);
                        foreach (var item in clientColors)
                        {
                            messageBuilder.Append(string.Format("                          {0}{1}:        {2}{3}{4}", item.Value, item.Key.Id, item.Key.RemoteEnd.Address, Color.Style.Reset, Def.NEWLINE));
                        }
                        messageBuilder.Append(Color.FG.Black + Color.BG.Gray +
                            "                                                                               " +
                            Color.Style.Reset + Def.NEWLINE);
                        client.Send(messageBuilder.ToString());
                    }
                    else if (string.IsNullOrWhiteSpace(clientInput))
                    {

                    }
                    else
                    {
                        string timestampPrefix = string.Format("{0} : {1} says: &X", DateTime.UtcNow, client.Id);
                        string selfPrefix = string.Format("^g&WSent!^k  You said: ");
                        string message = string.Format(clientColors[client] + clientInput + "&X");

                        clients.SendToAllExcept(Color.ParseTokens(timestampPrefix + message,true), client);
                        client.Send(Color.ParseTokens(selfPrefix + message,true));
                    }
                    clientInput = null;
                    client.Send(Def.NEWLINE + prompt);
                }
                
            }


        }

    }
}
