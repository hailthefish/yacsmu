﻿using System;
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
            Program.server.OnNewClientConnected += _NewClient;
            Program.OnUpdate += _Update;

            clientColors = new Dictionary<Client, string>();
            clients = Program.server.clients;

            Commands.ParamsAction who = Who;
            Commands.AddCommand("who", who);

            Commands.ParamsAction say = Say;
            Commands.AddCommand(new string[] { "'", "say" }, say);

            Commands.ParamsAction quit = Quit;
            Commands.AddCommand("quit", quit, fullMatch: true);

            Commands.ParamsAction beep = Beep;
            Commands.AddCommand("beep", beep, 1);

            Commands.ParamsAction recolor = Recolor;
            Commands.AddCommand("recolor", recolor);

            Commands.ParamsAction fart = Fart;
            Commands.AddCommand("fart", fart);

            Log.Information("SimpleChat loaded.");
        }

        private void Fart(ref Client sender, string[] args)
        {
            if (string.IsNullOrWhiteSpace(args[0]))
            {
                sender.Send("&GYou fart!&X");
                clients.SendToAllExcept(string.Format("&X{0}{1}&G farts!&X", clientColors[sender], sender.Id), sender);
            }
            else
            {
                bool parsedTarget = uint.TryParse(args[0], out uint targetId);
                if (!parsedTarget)
                {
                    sender.Send("&RInvalid target argument. Target must be a number.&X");
                }
                else
                {
                    Client target = clients.GetClientByID(targetId);

                    if (target == null)
                    {
                        sender.Send("&RInvalid target. Try using the WHO command.&X");
                    }
                    else if (sender != target)
                    {
                        target.Send(string.Format("&X{1}{0}&G farts on you!&X", sender.Id, clientColors[sender]));
                        sender.Send(string.Format("&GYou fart on {0}!&X", target.Id));
                        clients.SendToAllExcept(string.Format("&X{0}{1}&G farts on {2}{3}&G!&X", 
                            clientColors[sender], sender.Id, clientColors[target], target.Id),
                            new List<Client> { target, sender });
                    }
                    else
                    {
                        sender.Send("&YWhy would you want to fart on yourself?&X");
                    }
                }
            }
        }

        private void Recolor(ref Client client, string[] args)
        {
            string color = Color.RandomFG();
            clientColors[client] = Color.Tokens.mapANSI.FirstOrDefault(x => x.Value == color).Key;
            client.Send(string.Format("&X{0}This is your new color.&X",clientColors[client]));
        }

        private void Beep(ref Client sender, string[] args)
        {
            if (args.Length < 2)
            {
                sender.Send("&RSyntax: Beep <target ID #> <message>.");
            }
            else
            {
                bool parsedTarget = uint.TryParse(args[0], out uint targetId);
                if (!parsedTarget)
                {
                    sender.Send("&RInvalid target argument. Target must be a number.&X");
                }
                else
                {
                    Client target = clients.GetClientByID(targetId);

                    if (target == null)
                    {
                        sender.Send("&RInvalid target. Try using the WHO command.&X");
                    }
                    else if (sender != target)
                    {
                        if (args[1]!= null)
                        {
                            string message = args[1];
                            target.SendBell();
                            target.Send(string.Format("&Y{0} beeped you&w: {1}{2}&X", sender.Id, clientColors[sender], message));
                            sender.Send(string.Format("&YYou beeped {0}&w: {1}{2}&X", target.Id, clientColors[sender], message));
                        }
                        else
                        {
                            sender.Send("&RYou can't just beep someone with no message. Rude.&X");
                        }
                       
                    }
                    else
                    {
                        sender.Send("&YWhy would you want to beep yourself?&X");
                    }
                }



            }
        }

        private void Quit(ref Client client, string[] args)
        {
            client.Status = ClientStatus.Disconnecting;
            client.Send("^c&WGoodbye!&X");
            clients.SendToAllExcept(string.Format("{0}{1}&K has quit.&X", clientColors[client], client.Id), client);
        }


        private void Who(ref Client client, string[] args)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.Append("^k&w" +
                "--------------------------------- Simple Chat ---------------------------------" +
                Color.Style.Reset + Def.NEWLINE);
            foreach (var item in clientColors)
            {
                if (item.Key == client)
                {
                    messageBuilder.Append(string.Format("                          ^k{0}&U{1:D10}        {2}&u&X{3}",
                        item.Value, item.Key.Id, item.Key.RemoteEnd.Address, Def.NEWLINE));
                }
                else
                {
                    messageBuilder.Append(string.Format("                          ^k{0}{1:D10}        {2}&X{3}",
                        item.Value, item.Key.Id, item.Key.RemoteEnd.Address, Def.NEWLINE));
                }
                
            }
            messageBuilder.Append("^k&w" +
                "-------------------------------------------------------------------------------" +
                Color.Style.Reset + Def.NEWLINE);
            client.Send(messageBuilder.ToString());
        }

        private void Say(ref Client client, params object[] args)
        {
            if (args[0] != null)
            {
                Log.Verbose("Say invoked with {0} params.", args.Length);
                for (int i = 0; i < args.Length; i++)
                {
                    Log.Verbose("Param {0}: Type: {1}, Content: \'{2}\'", i, args[i].GetType().ToString(), args[i].ToString());
                }
                string clientInput = (string)args[0];
                string otherPrefix = string.Format("&w{0} says: &X", client.Id);
                string selfPrefix = "&wYou said: ";
                string message = string.Format(clientColors[client] + clientInput + "&X");

                clients.SendToAllExcept(otherPrefix + message, client);
                client.Send(selfPrefix + message);
            }
            else
            {
                Log.Verbose("Say invoked with 0 arguments.");
                client.Send("&RYou can't just say nothing.&X");
            }
        }

        internal string GetColor(Client client)
        {
            return (clientColors != null && clientColors.Count > 0 && clientColors.ContainsKey(client)) ?
                clientColors[client] : "&w";
        }

        private void _NewClient(object Sender, NewClientEventArgs e)
        {
            var client = e.Client;
            // Generate a random control code, and find its token, then add the token to our dictionary
            string color = Color.RandomFG();
            clientColors.Add(client, Color.Tokens.mapANSI.FirstOrDefault(x => x.Value == color).Key);

            // And send the greeting
            client.Send(string.Format("Simple Chat running on {0}:{1}", Program.server.Host, Program.server.Port));
            Commands.SendPrompt(client);
            clients.SendToAllExcept(string.Format("&X{0}{1}&W has joined.&X",clientColors[client],client.Id), client);
        }

        internal void _Update(object sender, EventArgs e)
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
            
        }

    }
}
