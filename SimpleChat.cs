using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;

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
            Commands.ParamsAction who = Who;
            Commands.AddCommand("who", this, who);
            Commands.ParamsAction say = Say;
            Commands.AddCommand("say", this, say);


            Log.Information("SimpleChat running.");
        }


        private void Who(object obj, object[] args)
        {
            Client client = (Client)args[0];
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append("&k^w" +
                "                                  Simple Chat                                  " +
                Color.Style.Reset + Def.NEWLINE);
            foreach (var item in clientColors)
            {
                messageBuilder.Append(string.Format("                          {0}{1}:        {2}{3}{4}",
                    item.Value, item.Key.Id, item.Key.RemoteEnd.Address, Color.Style.Reset, Def.NEWLINE));
            }
            messageBuilder.Append("&k^w" +
                "                                                                               " +
                Color.Style.Reset + Def.NEWLINE);
            client.Send(messageBuilder.ToString());
        }

        private void Say(object obj, params object[] args)
        {
            Log.Verbose("Say invoked with {0} params.", args.Length);
            for (int i = 0; i < args.Length; i++)
            {
                Log.Verbose("Param {0}: Type: {1}, Content: \'{2}\'", i, args[i].GetType().ToString(),args[i].ToString());
            }
            Client client = (Client)args[0];
            string clientInput = (string)args[1];
            string timestampPrefix = string.Format("{0} : {1} says: &X", DateTime.UtcNow, client.Id);
            string selfPrefix = "^g&WSent!^k  You said: ";
            string message = string.Format(clientColors[client] + clientInput + "&X");

            clients.SendToAllExcept(timestampPrefix + message, client);
            client.Send(selfPrefix + message);
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
                // Assign a random color for a newly connected client & send them a greeting.
                if (!clientColors.ContainsKey(client)) //If we don't have a client color entry for them they must be new
                {
                    // Generate a random control code, and find its token, then add the token to our dictionary
                    clientColors.Add(client, Color.Tokens.mapANSI.FirstOrDefault(x => x.Value == Color.RandomFG()).Key);

                    // And send the greeting
                    client.Send(string.Format("Simple Chat running on {0}:{1}", Program.server.Host, Program.server.Port));
                    Commands.SendPrompt(client);
                }
                
                
            }


        }

    }
}
