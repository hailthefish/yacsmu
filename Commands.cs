using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using System.Text;

namespace yacsmu
{
    internal struct Command
    {
        internal object CallObject;
        internal Commands.ParamsAction MethodDelegate;
        internal bool FullMatch;
        

        internal Command(object obj, Commands.ParamsAction del, bool fullMatch)
        {
            FullMatch = fullMatch;
            CallObject = obj;
            MethodDelegate = del;
        }
    }

    static class Commands
    {
        public delegate void ParamsAction(object obj, object[] arguments);

        private const string prompt = ">> ";

        private static Dictionary<string, Command> commandDict = new Dictionary<string, Command>();
        private static List<string> commandList = null;

        public static bool AddCommand
            (string commandString, object obj, ParamsAction commandMethod, bool fullMatch = false)
        {
            bool ret;
            if (ret = commandDict.TryAdd(commandString.ToLower(), new Command(obj, commandMethod, fullMatch)))
            {
                Log.Debug("Added command string \"{0}\" from {1}.", commandString.ToLower(), obj);
            }
            else
            {
                Log.Debug("Failed to add command string \"{0}\" from {1}.", commandString.ToLower(), obj);
            }
            return ret;
        }

        public static void AddCommand
            (string[] commandStrings, object obj, ParamsAction commandMethod, bool fullMatch = false, bool caseSensitive = false)
        {
            for (int i = 0; i < commandStrings.Length; i++)
            {
                if (commandDict.TryAdd(commandStrings[i].ToLower(), new Command(obj, commandMethod, fullMatch)))
                {
                    Log.Debug("Added command string \"{0}\" from {1}.", commandStrings[i].ToLower(), obj);
                }
                else
                {
                    Log.Debug("Failed to add command string \"{0}\" from {1}.", commandStrings[i].ToLower(), obj);
                }
            }

        }

        internal static void Ready()
        {
            commandList = new List<string>(commandDict.Keys);
            commandList.Sort();
        }

        public static void SendPrompt(Client client)
        {
            client.Send(Def.NEWLINE + prompt);
        }

        internal static void ParseInputs()
        {
            List<Client> clientList = Program.server.clients.GetClientList();
            foreach (var client in clientList)
            {
                if (client.inputQueue.Count > 0)
                {
                    string clientInput = client.inputQueue.Dequeue();

                    string command;
                    if (clientInput != string.Empty)
                    {
                        command = char.IsPunctuation(clientInput.First()) ? 
                            clientInput.First().ToString() : clientInput.Split(" ").First().ToLower();
                        string commandText = clientInput.TrimStart(command.ToCharArray()).Trim();
                        Log.Verbose("Input: {0} sent {1}. Parsed to: \'{2}\' + \'{3}\'",
                            client.RemoteEnd.Address, clientInput, command, commandText);
                        object[] arguments = new object[] { client, commandText };
                        if (commandDict.ContainsKey(command))
                        {
                            Command c = commandDict[command];
                            c.MethodDelegate.Invoke(c.CallObject, arguments);
                        }
                        else
                        {
                            int index = commandList.BinarySearch(command);
                            index = ~index; // We won't get here if there's an exact match, so we know we have the XOR of the closest match
                            if (index >= 0 && index < commandList.Count && commandList[index].StartsWith(command))
                            {
                                string match = commandList[index];
                                Command c = commandDict[match];
                                if (!c.FullMatch)
                                {
                                    c.MethodDelegate.Invoke(c.CallObject, arguments);
                                }
                                else
                                {
                                    client.Send(string.Format("&RPlease type out '{0}' completely.&X", match));
                                }

                            }
                            else
                            {
                                client.Send(string.Format("&RSorry, '{0}' doesn't match any known commands.&X", Color.Escape(command)));
                            }
                        }
                    }
                    

                    // Done Parsing
                    clientInput = null;
                    SendPrompt(client);
                }
            }
        }

    }
}
