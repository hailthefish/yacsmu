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

            var loggerConfig = new LoggerConfiguration();
            try
            {
                bool consoleParseSuccess = bool.TryParse((configuration["Logging:LogToConsole"]), out bool logToConsole);

                if (consoleParseSuccess && logToConsole)
                {
                    loggerConfig.WriteTo.Console();
                    Console.WriteLine("Logging to console.");
                }
                else if (!consoleParseSuccess)
                {
                    loggerConfig.WriteTo.Console();
                    Console.WriteLine("Invalid config option for LogToConsole! Should be True or False. " +
                        "Defaulting to console logging ENABLED.");
                }
                else Console.WriteLine("Logging to console DISABLED.");

                bool tofileParseSuccess = bool.TryParse((configuration["Logging:LogToFile"]), out bool logToFile);

                if (tofileParseSuccess && logToFile)
                {
                    string pathString = configuration["Logging:LogFilePath"];
                    if (pathString.Length > 0 && Uri.IsWellFormedUriString(pathString, UriKind.RelativeOrAbsolute))
                    {
                        Console.Write("Logging to file path: {0}", pathString);
                        loggerConfig.WriteTo.File(pathString);
                    }
                    else
                    {
                        Console.WriteLine("Invalid log file path! Logging to file disabled.");
                    }
                }
                else if (!tofileParseSuccess) Console.WriteLine("Invalid config option for LogToFile! " +
                    "Should be True or False. Logging to file disabled.");

                bool logLevelParseSuccess = Enum.TryParse(configuration["Logging:MinimumLogLevel"],true,out LogEventLevel logEventLevel);
                if (logLevelParseSuccess)
                {
                    Console.WriteLine("Minimum Log Level: {0}.", logEventLevel);
                    loggerConfig.MinimumLevel.Is(logEventLevel);
                }
                else
                {
                    loggerConfig.MinimumLevel.Information();
                    Console.WriteLine("Invalid config option for MinimumLogLevel! " +
                        "Should be: Verbose, Debug, Information, Warning, Error, or Fatal. Minimum log level set to Information.");
                }
                
                Log.Logger = loggerConfig.CreateLogger();
            }
            catch (Exception)
            {
                throw;
            }



            Log.Information("Logging started.");
        }
    }
}
