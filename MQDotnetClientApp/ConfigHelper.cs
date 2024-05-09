using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQDotnetClientApp
{
    internal static class ConfigHelper
    {
        private static readonly string[] _commandArgs = { "configPath", "console" };
        public static IDictionary<string, string>? GetDictionary(IConfigurationSection configurationSection)
        {
            var configs = configurationSection.GetChildren();
            if (configs != null && configs.Any())
            {
                return configs.ToDictionary(x => x.Key, x => x.Value ?? string.Empty);
            }

            return configurationSection.GetChildren()?.ToDictionary(x => x.Key, x => x.Value ?? string.Empty);
        }

        public static IConfigurationRoot SetConfiguration()
        {
            var build = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false)
                    .AddEnvironmentVariables()
                    .Build();

        
            return build;
        }
        
    }
}
