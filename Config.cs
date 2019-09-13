using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Ini;


namespace yacsmu
{
    internal static class Config 
    {
        internal static IConfigurationRoot configuration;

        internal static void ConfigureSettings()
        {
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddIniFile("config.ini", optional: false, reloadOnChange: false)
                .Build();
        }


    }
}
