using System;
using System.Diagnostics;
using System.Threading;

namespace yacsmu
{
    class Program
    {
        private const long MAIN_TICKRATE = 250; // milliseconds, how often the main loop runs.

        private static void Startup()
        {
            Console.WriteLine("Loading Configuration...");
            Config.ConfigureSettings();
            Console.WriteLine("Configuration Loaded.");


            // Last thing in startup
            Console.WriteLine("Preparing to start server on port " + Config.configuration["server:port"]);
            Console.ReadLine();
        }

        private static void MainLoop()
        {
            var timer = new Stopwatch();
            timer.Start();
            Console.WriteLine("Running.");
            bool running = true;
            int counter = 0;
            while (running)
            {
                counter++;

                // Background/connection stuff happens here?

                if (timer.ElapsedMilliseconds >= MAIN_TICKRATE)
                {

                    // Game Update Stuff Happens Here


                    Console.WriteLine("Tick: " + timer.ElapsedMilliseconds + ":" + counter);
                    timer.Restart();
                    counter = 0;
                }
            }
        }

        static void Main(string[] args)
        {
            Startup();

            MainLoop();

            //Shutdown goes here
        }
    }
}
