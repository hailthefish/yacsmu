using System;
using System.Diagnostics;
using System.Threading;

namespace yacsmu
{
    class Program
    {
        private const long MAIN_TICKRATE = 250; // milliseconds, how often the main loop runs.

        private static Server server;
        private static bool running = true;

        static void Main(string[] args)
        {
            Console.WriteLine("Loading Configuration...");
            Config.LoadConfig();

            // DB stuff will go here eventually

            // Last things in startup
            Console.WriteLine("Ready. Press enter to start:");
            Console.ReadLine();
            server = new Server();
            server.Start();
            Console.WriteLine("Running. Press 'q' to stop.");

            
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
                    }
                }
                

                if (timer.ElapsedMilliseconds >= MAIN_TICKRATE)
                {

                    // Game Update Stuff Happens Here


                    server.CheckAlive();
                    
                    Console.Write("Tick. ");
                    if (server.connectedClients.Count > 0)
                    {
                        Console.WriteLine(server.connectedClients.Count + " connections.");
                        foreach (var client in server.connectedClients)
                        {
                            server.DirectSendToClient(client.Value,string.Format("{1}: {0} connections.", DateTime.UtcNow, server.connectedClients.Count));
                        }

                    }
                    else Console.WriteLine();


                    if (timer.ElapsedMilliseconds > (MAIN_TICKRATE * 2))
                    {
                        Console.WriteLine("LAG: " + timer.ElapsedMilliseconds + "ms : " + counter + " cycles");
                    }

                    timer.Restart();
                    counter = 0;
                }
            }
            Console.WriteLine("Stopping...");
            
            server.Stop();
            //Shutdown goes here

            Console.WriteLine("Done.");
            Console.ReadLine();
        }
    }
}
