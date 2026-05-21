using System;
using System.Collections.Generic;
using System.Text;

namespace ApiTestAssistant.Cli
{
    public static class CliHelpPrinter
    {
        public static void PrintUsage()
        {
            Console.WriteLine("Usage: dotnet run --project <project> -- [options]\n");
            Console.WriteLine("Options:");
            Console.WriteLine("  -c, --config <path>       Path to appsettings.json (default: look in cwd or project dir)");
            Console.WriteLine("  -u, --openapi-url <url>   Override OpenAPI URL");
            Console.WriteLine("  --apikey <key>            LLM API key (or use OPENROUTER_API_KEY env var)");
            Console.WriteLine("  -o, --output <file>       Output file path (CSV, Excel will be in same folder with same name)");
            Console.WriteLine("  -h, --help                Show this help");
        }
    }
}
