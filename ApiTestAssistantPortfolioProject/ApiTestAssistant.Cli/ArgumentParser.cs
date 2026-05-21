using System;
using System.Collections.Generic;
using System.Text;

namespace ApiTestAssistant.Cli
{
    public class CliOptions
    {
        public bool ShowHelp { get; set; }
        public string? ConfigPath { get; set; }
        public string? OpenApiUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? OutputFile { get; set; }
    }

    public static class ArgumentParser
    {
        public static CliOptions Parse(string[] args)
        {
            var options = new CliOptions();
            for (int i = 0; i < args.Length; i++)
            {
                var a = args[i];
                switch (a)
                {
                    case "-h":
                    case "--help":
                        options.ShowHelp = true; break;
                    case "-c":
                    case "--config":
                        if (i + 1 < args.Length) options.ConfigPath = args[++i]; break;
                    case "-u":
                    case "--openapi-url":
                        if (i + 1 < args.Length) options.OpenApiUrl = args[++i]; break;
                    case "--apikey":
                        if (i + 1 < args.Length) options.ApiKey = args[++i]; break;
                    case "-o":
                    case "--output":
                        if (i + 1 < args.Length) options.OutputFile = args[++i]; break;
                }
            }
            return options;
        }
    }
}
