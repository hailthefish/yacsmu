using System;
using System.Diagnostics;
using System.Threading;

namespace yacsmu
{
    class Program
    {
        private const long MAIN_TICKRATE = 250; // milliseconds, how often the main loop runs.

        private static Server server;

        private static void MainLoop()
        {
            bool running = true;
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
                

                // Background/connection stuff happens here?


                if (timer.ElapsedMilliseconds >= MAIN_TICKRATE)
                {

                    // Game Update Stuff Happens Here


                    if (timer.ElapsedMilliseconds > (MAIN_TICKRATE + 100))
                    {
                        Console.WriteLine("LAG: " + timer.ElapsedMilliseconds + "ms : " + counter + " cycles");
                    }
                    
                    timer.Restart();
                    counter = 0;
                }
            }
            Console.WriteLine("Stopping...");
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Loading Configuration...");
            Config.ConfigureSettings();



            // Last things in startup
            Console.WriteLine("Ready. Press enter to start:");
            Console.ReadLine();
            server = new Server();
            server.Start();
            Console.WriteLine("Running. Press 'q' to stop.");
            server.WillAcceptConnections = true;

            MainLoop();

            server.Stop();
            //Shutdown goes here
        }
    }
}
