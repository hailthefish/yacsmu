using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace yacsmu
{
    internal static class Config 
    {
        internal static IConfigurationRoot configuration;

        internal static void LoadConfig()
        {
            Console.WriteLine("Loading Configuration...");
            
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddIniFile("config.ini", optional: false, reloadOnChange: false)
                .Build();

            Console.WriteLine("Configuration Loaded.");
        }

        internal static void SetupLogging()
        {
            Console.WriteLine("Starting logging...");

            Serilog.Debugging.SelfLog.Enable(TextWriter.Synchronized(Console.Error));

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                //.WriteTo.File("logs\\myapp.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Logging started.");
        }
    }
}
