using System.Collections.Generic;
using System.Linq;
using Serilog;
using System.Text.RegularExpressions;
using System;

namespace yacsmu
{
    internal struct Command
    {
        internal Commands.ParamsAction MethodDelegate;
        internal bool FullMatch;
        internal int ReservedArguments;
        

        internal Command(Commands.ParamsAction del, int reservedArguments = 0, bool fullMatch = false)
        {
            MethodDelegate = del;
            ReservedArguments = reservedArguments;
            FullMatch = fullMatch;
        }
    }

    static class Commands
    {
        public delegate void ParamsAction(ref Client client, string[] arguments);

        private static Dictionary<string, Command> commandDict = new Dictionary<string, Command>(StringComparer.OrdinalIgnoreCase);
        private static List<string> commandList = null;

        public static bool AddCommand
            (string commandString, ParamsAction commandMethod, int reservedArguments = 0, bool fullMatch = false)
        {
            bool ret;
            if (ret = commandDict.TryAdd(commandString.ToLower(), new Command(commandMethod, reservedArguments, fullMatch)))
            {
                Log.Debug("Added command \"{0}\" for {1} from {2} with {3} arguments.",
                    commandString.ToLower(), commandMethod.Method.Name, commandMethod.Target, reservedArguments);
            }
            else
            {
                Log.Debug("Failed to add command \"{0}\" for {1} from {2} with {3} arguments.",
                    commandString.ToLower(), commandMethod.Method.Name, commandMethod.Target, reservedArguments);
            }
            return ret;
        }

        public static void AddCommand
            (string[] commandStrings, ParamsAction commandMethod, int reservedArguments = 0, bool fullMatch = false)
        {
            for (int i = 0; i < commandStrings.Length; i++)
            {
                if (commandDict.TryAdd(commandStrings[i].ToLower(), new Command(commandMethod, reservedArguments)))
                {
                    Log.Debug("Added command \"{0}\" for {1} from {2} with {3} arguments.",
                        commandStrings[i].ToLower(), commandMethod.Method.Name, commandMethod.Target, reservedArguments);
                }
                else
                {
                    Log.Debug("Failed to add command \"{0}\" for {1} from {2} with {3} arguments.",
                        commandStrings[i].ToLower(), commandMethod.Method.Name, commandMethod.Target, reservedArguments);
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
            if (Program.simpleChat != null)
            {
                client.Send(string.Format("&X{0}>{2}{1}&X> ", Def.NEWLINE, client.Id, Program.simpleChat.GetColor(client)));
            }
            else
            {
                client.Send(string.Format("&X{0}>{1}> ", Def.NEWLINE, client.Id));
            }
            
        }

        private static string[] ParseArguments(ref Client client, Command command, string clientInput = null)
        {
            
            string[] arguments;
            if (command.ReservedArguments > 0 && !string.IsNullOrWhiteSpace(clientInput) /*&& Regex.IsMatch(clientInput, @"(\S+)")*/)
            {
                arguments = new string[command.ReservedArguments + 1];
                int firstSpace = clientInput.IndexOf(' ');
                if (firstSpace > 0)
                {
                    for (int i = 0; i < command.ReservedArguments; i++)
                    {
                        Log.Debug("Client Input: \"{clientInput}\", first space at: {firstSpace}.", clientInput, firstSpace);
                        if (firstSpace > 0)
                        {
                            arguments[i] = clientInput.Substring(0, clientInput.IndexOf(' ')).Trim();
                            clientInput = clientInput[firstSpace..].TrimStart();
                        }
                        else
                        {
                            arguments[i] = clientInput;
                        }
                        firstSpace = clientInput.IndexOf(' ');
                    }
                    arguments[command.ReservedArguments] = clientInput;
                }
                else
                {
                    arguments[0] = clientInput;
                }
                Log.Debug("Parsing command arguments from {client}: \"{clientInput}\" as: {arguments}.", client.Id, clientInput, arguments);
            }
            else
            {
                arguments = new string[] { clientInput };
                
            }
            
            return arguments;
        }

        internal static void ParseInputs()
        {
            List<Client> clientList = Program.server.clients.GetClientList();
            for (int i = 0; i < clientList.Count; i++)
            {
                Client client = clientList[i];
                if (client.inputQueue.Count > 0)
                {
                    string clientInput = client.inputQueue.Dequeue();
                    

                    
                    if (!string.IsNullOrWhiteSpace(clientInput))
                    {
                        
                        string command;
                        string remainder = null;

                        if (char.IsPunctuation(clientInput.First()))
                        {
                            command = clientInput.First().ToString();
                            remainder = clientInput.TrimStart(clientInput.First());
                        }
                        else
                        {

                            int firstSpace = clientInput.IndexOf(' ');
                            if (firstSpace > 0)
                            {
                                command = clientInput.Substring(0, clientInput.IndexOf(' '));
                                remainder = clientInput.Substring(firstSpace + 1);
                            }
                            else command = clientInput;
                        }
                        Log.Debug("Parsing command {clientInput} from {client} as: [{command}] + [{remainder}].", clientInput, client.Id, command, remainder);

                        if (commandDict.ContainsKey(command))
                        {
                            Command c = commandDict[command];
                            c.MethodDelegate.Invoke(ref client, ParseArguments(ref client, c, remainder));
                        }
                        else
                        {
                            int index = commandList.BinarySearch(command);
                            // We won't get here if there's an exact match, so we know we have the XOR of the index of the closest match
                            index = ~index;
                            if (index >= 0 && index < commandList.Count && commandList[index].StartsWith(command))
                            {
                                string match = commandList[index];
                                Command c = commandDict[match];
                                if (!c.FullMatch)
                                {
                                    c.MethodDelegate.Invoke(ref client, ParseArguments(ref client,c,remainder));
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
                    SendPrompt(client);
                }
            }
        }

    }
}
