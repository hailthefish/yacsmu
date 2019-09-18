using System;
using System.Diagnostics;
using System.Threading;

namespace yacsmu
{
    class Program
    {
        private const long MAIN_TICKRATE = 250; // milliseconds, how often the main loop runs.

        internal static Server server;
        private static bool running = true;

        static void Main(string[] args)
        {
            Console.WriteLine("Loading Configuration...");
            Config.LoadConfig();

            // DB stuff will go here eventually

            Console.WriteLine("Ready. Press enter to start:");
            Console.ReadLine();
            server = new Server();
            server.Start();
            Console.WriteLine("Running. Press 'q' to stop.");

            // Load client-list dependent stuff after here

            SimpleChat simpleChat = new SimpleChat();

            // Last part of startup
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
                    server.CheckAlive();
                    server.clients.GetAllInput();

                    // Game Update Stuff Happens Here

                    simpleChat.Update();
                    

                    //
                    Console.Write("Tick. ");
                    if (server.clients.Count > 0)
                    {
                        Console.WriteLine(server.clients.Count + " connections.");
                        //server.clients.SendToAll(Color.RandomFG() + string.Format("{0}: {1} clients connected.", DateTime.UtcNow, server.clients.Count) + Color.Reset);

                    }
                    else Console.WriteLine();


                    if (timer.ElapsedMilliseconds > (MAIN_TICKRATE * 2))
                    {
                        Console.WriteLine("LAG: " + timer.ElapsedMilliseconds + "ms : " + counter + " cycles");
                    }
                    
                    server.clients.FlushAll();

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
