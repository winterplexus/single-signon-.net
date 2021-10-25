//
//  ApplicationSettings.cs
//
//  Copyright (c) Wiregrass Code Technology 2021
//
using System;
using Microsoft.Extensions.Configuration;

[assembly: CLSCompliant(true)]
namespace Authenticator.Utility
{
    public static class ApplicationSettings
    {
        private const string settingsPath = "appsettings.json";
        private const string settingsSection = "appSettings";

        public static bool GetBooleanValue(string name)
        {
            var configuration = GetConfiguration(null);

            return !string.IsNullOrEmpty(configuration[name]) && configuration[name].Equals("T", StringComparison.OrdinalIgnoreCase);
        }

        public static bool GetnBooleanValue(string section, string name)
        {
            var configuration = GetConfiguration(section);

            return !string.IsNullOrEmpty(configuration[name]) && configuration[name].Equals("T", StringComparison.OrdinalIgnoreCase);
        }

        public static int GetNumberValue(string name)
        {
            var configuration = GetConfiguration(null);

            return int.TryParse(configuration[name], out int number) ? number : 0;
        }

        public static int GetNumberValue(string section, string name)
        {
            var configuration = GetConfiguration(section);

            return int.TryParse(configuration[name], out int number) ? number : 0;
        }

        public static string GetStringValue(string name)
        {
            var configuration = GetConfiguration(null);

            return !string.IsNullOrEmpty(configuration[name]) ? configuration[name] : string.Empty;
        }

        public static string GetStringValue(string section, string name)
        {
            var configuration = GetConfiguration(section);

            return !string.IsNullOrEmpty(configuration[name]) ? configuration[name] : string.Empty;
        }

        private static IConfigurationSection GetConfiguration(string section)
        {
            string sectionName = settingsSection;

            if (!string.IsNullOrEmpty(section))
            {
                sectionName = section;
            }

            var configurationRoot = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile(settingsPath)
                .Build();

            return configurationRoot.GetSection(sectionName);
        }
    }
}