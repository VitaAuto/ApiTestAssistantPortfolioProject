using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Text.Json;
using System.Collections.Generic;

namespace ApiTestAssistant.Core.Utils
{
    public static class ConfigLoader
    {
        public static string? FindConfigPath(string? configPathArg)
        {
            var candidates = new List<string>
            {
                configPathArg ?? string.Empty,
                Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"),
                Path.Combine(AppContext.BaseDirectory, "appsettings.json"),
                Path.Combine(AppContext.BaseDirectory, "..", "appsettings.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "appsettings.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "appsettings.json")
            };

            foreach (var cand in candidates)
            {
                if (string.IsNullOrEmpty(cand)) continue;
                try
                {
                    var full = Path.GetFullPath(cand);
                    if (File.Exists(full))
                    {
                        return full;
                    }
                }
                catch { }
            }
            return null;
        }

        public static Dictionary<string, Dictionary<string, string>> Load(string configPath)
        {
            var configText = File.ReadAllText(configPath);
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(configText) ?? new();
        }

        public static string? GetConfigValue(Dictionary<string, Dictionary<string, string>> config, string section, string key)
        {
            if (config.ContainsKey(section) && config[section].ContainsKey(key))
                return config[section][key];
            return null;
        }
    }
}
