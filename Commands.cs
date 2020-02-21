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

        internal Command(object obj, Commands.ParamsAction del)
        {
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

        public static bool AddCommand(string commandString, object obj, ParamsAction commandMethod)
        {
            bool ret;
            if (ret = commandDict.TryAdd(commandString, new Command(obj, commandMethod)))
            {
                Log.Debug("Added command \"{0}\" from caller {1}.", commandString, obj);
            }
            else
            {
                Log.Debug("Failed to add command \"{0}\" from caller {1}.", commandString, obj);
            }
            return ret;
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
                   
                    string command = clientInput.Split(" ").First();
                    string commandText = clientInput.TrimStart(command.ToCharArray()).Trim();
                    Log.Verbose("Input: {0} sent {1}. Parsed to: \'{2}\' + \'{3}\'", client.RemoteEnd.Address, clientInput, command, commandText);
                    object[] arguments = new object[] {client, commandText };
                    if (clientInput != string.Empty && commandDict.ContainsKey(command))
                    {
                        Command c = commandDict[command];
                        c.MethodDelegate.Invoke(c.CallObject, arguments);
                    }
                    else if (clientInput != string.Empty)
                    {
                        int index = commandList.BinarySearch(command);
                        if ((~index) >= 0 && (~index) < commandList.Count && commandList[~index].StartsWith(command))
                        {
                            Command c = commandDict[commandList[~index]];
                            c.MethodDelegate.Invoke(c.CallObject, arguments);
                        }
                        else
                        {
                            client.Send("&RSorry, '" + Color.Escape(command) + "' doesn't match any known commands.&X");
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
