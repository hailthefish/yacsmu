using System;
using System.Diagnostics;
using System.Threading;
using Serilog;
using Serilog.Events;

namespace yacsmu
{
    class Program
    {
        private const long MAIN_TICKRATE = 250; // milliseconds, how often the main loop runs.

        internal static Server server;
        internal static SimpleChat simpleChat;
        private static bool running = true;

        static void Main(string[] args)
        {
            Config.LoadConfig();
            Config.SetupLogging();
            
            bool logLevelIsVerbose = Log.IsEnabled(LogEventLevel.Verbose);

            // DB stuff will go here eventually

            Console.WriteLine("Ready. Press enter to start:");
            Console.ReadLine();
            server = new Server();
            server.Start();
            Console.WriteLine("Running. Press 'q' to stop.");

            // Load client-list dependent stuff after here

            simpleChat = new SimpleChat();



            // Last part of startup
            Commands.Ready();
            int counter = 0;
            var timer = new Stopwatch();
            timer.Start();

            while (running)
            {
                counter++;

                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(true).KeyChar == 'q')
                    {
                        running = false;
                        Log.Information("Shutdown started from console.");
                        server.clients.SendToAll("&RShutdown initiated from Console.&X");
                    }
                }
                

                if (timer.ElapsedMilliseconds >= MAIN_TICKRATE)
                {
                    server.CheckConnectionsAlive(); // Check server connections and flag disconnected ones for client removal, then remove them

                    Log.Verbose("Tick. {clientsConnected} clients connected.", server.clients.Count);
                    if (timer.ElapsedMilliseconds > (MAIN_TICKRATE * 2))
                    {
                        Log.Warning("LAG: {elapsedMilliseconds} ms : {cycles} cycles!", timer.ElapsedMilliseconds, counter);
                    }

                    server.clients.GetAllInput();
                    Commands.ParseInputs();

                    // Game Update Stuff Happens Here


                    simpleChat.Update();


                    // End of this game update loop, send waiting data.
                    server.clients.FlushAll();
                    timer.Restart();
                    counter = 0;
                }
                Thread.Sleep(10);
            }

            //Shutdown goes here

            foreach (var client in server.clients.GetClientList())
            {
                client.Status = ClientStatus.Disconnecting;
            }
            server.clients.FlushAll();
            server.Stop();

            Log.Information("Shutdown complete.");
            Log.CloseAndFlush();

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
