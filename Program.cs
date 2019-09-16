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
                    else
                    {
                        foreach (var client in server.connectedClients)
                        {
                            client.Value.AddOutput("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec cursus et dolor vitae pellentesque. Aenean eu eleifend nulla. Nullam tristique risus nisi, sit amet blandit dui vestibulum in. Morbi ultricies aliquam orci sit amet blandit. Aliquam malesuada, elit nec tincidunt dapibus, velit nulla eleifend velit, vel condimentum dui mauris non sapien. Donec faucibus sem eu viverra tincidunt. Aliquam vestibulum elit elit, et ullamcorper risus ultrices et. Mauris eget efficitur urna.\r\n");
                            client.Value.AddOutput("\r\nSuspendisse in dolor vehicula, consequat augue quis, interdum odio. Pellentesque facilisis sapien sit amet sapien volutpat ultricies. Donec ac libero id dui luctus tincidunt. Quisque ornare ipsum a dolor tincidunt, a aliquam orci pulvinar. Phasellus vestibulum eget arcu non congue. Etiam ligula dui, dapibus placerat interdum vestibulum, aliquet elementum neque. Praesent suscipit magna et magna blandit, sit amet dictum erat finibus. Fusce venenatis risus a justo laoreet, a vulputate tellus elementum.\r\n");
                            client.Value.AddOutput("\r\nSed sed arcu a ex ornare commodo non vitae sem. Mauris luctus nisl non fringilla iaculis. Phasellus gravida nisl metus, mollis ultricies libero elementum id. Nullam nulla diam, mollis ut augue ut, lobortis consectetur leo. Donec blandit varius dictum. Donec turpis est, iaculis sed elit eu, eleifend rutrum metus. Mauris vel lectus porttitor, efficitur ipsum nec, viverra libero. Nunc massa enim, mollis non facilisis vel, bibendum sit amet elit. Nam volutpat turpis ante, ut faucibus erat consequat in. Sed pellentesque leo sit amet velit pharetra venenatis. Nullam vel molestie lorem, vitae tempor sapien. Aliquam erat volutpat. Pellentesque eu urna ac nisl interdum porttitor ac nec purus. Sed quis varius elit. In posuere, est vitae sodales interdum, velit quam molestie dui, in egestas ex dolor suscipit nisl.\r\n");

                            while (client.Value.outputBuilder.Length > 0)
                            {
                                client.Value.SendOutput();
                            }
                        }
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
                            client.Value.AddOutput(string.Format("{0}: {1} clients connected.", 
                                DateTime.UtcNow, server.connectedClients.Count)+ Def.NEWLINE);
                        }

                    }
                    else Console.WriteLine();


                    if (timer.ElapsedMilliseconds > (MAIN_TICKRATE * 2))
                    {
                        Console.WriteLine("LAG: " + timer.ElapsedMilliseconds + "ms : " + counter + " cycles");
                    }

                    if (server.connectedClients.Count > 0)
                    {
                        foreach (var client in server.connectedClients)
                        {
                            client.Value.SendOutput();
                        }

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
