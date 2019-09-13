using System;

namespace yacsmu
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Loading Configuration...");
            Config.ConfigureSettings();
            Console.WriteLine("Configuration Loaded.");
            Console.WriteLine("Preparing to start server on port " + Config.configuration["server:port"]);
        }
    }
}
