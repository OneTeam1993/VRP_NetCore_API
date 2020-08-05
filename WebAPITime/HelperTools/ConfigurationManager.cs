using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace WebAPITime.HelperTools
{
    static class ConfigurationManager
    {
        public static IConfiguration AppSettings { get; }
        static ConfigurationManager()
        {
            AppSettings = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();
        }
    }
}
