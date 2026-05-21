using ApiTestAssistant.Cli;
using ApiTestAssistant.Common;
using ApiTestAssistant.Core.Models;
using ApiTestAssistant.Core.Services;
using ApiTestAssistant.Core.Utils;
using ApiTestAssistant.LLM;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.EnvironmentVariables;

class Program
{
    static async Task Main(string[] args)
    {
        var options = ArgumentParser.Parse(args);
        if (options.ShowHelp)
        {
            CliHelpPrinter.PrintUsage();
            return;
        }

        // 1. Configuration
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(options.ConfigPath ?? "appsettings.json", optional: true)
            .AddEnvironmentVariables();
        var config = configBuilder.Build();

        // 2. Logger initialization
        var configPath = options.ConfigPath ?? "appsettings.json";
        Logger.Init(Path.GetDirectoryName(configPath));
        Logger.Info($"Using config: {configPath}");

        // 3. IConfiguration and parameters
        var openApiUrl = options.OpenApiUrl
            ?? config["OpenApi:Url"]
            ?? Environment.GetEnvironmentVariable("OPENAPI_URL");
        openApiUrl = OpenApiUrlUtils.NormalizeOpenApiUrl(openApiUrl);

        var userName = config["OpenApi:UserName"] ?? Environment.GetEnvironmentVariable("OPENAPI_USERNAME");
        var userPassword = config["OpenApi:UserPassword"] ?? Environment.GetEnvironmentVariable("OPENAPI_USERPASSWORD");

        var openrouterUrl = config["Llm:OpenRouterUrl"] ?? Environment.GetEnvironmentVariable("OPENROUTER_API_URL");
        var llmApiKey = options.ApiKey
            ?? config["Llm:ApiKey"]
            ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
        var cfgModel = config["Llm:OpenRouterModel"] ?? Environment.GetEnvironmentVariable("OPENROUTER_MODEL") ?? "openrouter/free";

        Logger.Info($"Using OpenRouter client");
        Logger.Info($"LLM API key present: {(string.IsNullOrEmpty(llmApiKey) ? "no" : "yes")}");
        Logger.Info($"OpenRouter URL: {openrouterUrl}");
        Logger.Info($"OpenRouter model: {cfgModel}");

        // 4. Parameters validation
        if (string.IsNullOrEmpty(openApiUrl))
        {
            Logger.Error("OpenAPI URL not provided. Set in config (OpenApi:Url), use --openapi-url or set OPENAPI_URL env var.");
            return;
        }
        if (string.IsNullOrEmpty(options.OutputFile))
        {
            Logger.Error("Output file path not provided. Use --output or set in script.");
            return;
        }
        if (string.IsNullOrWhiteSpace(llmApiKey))
        {
            Logger.Error("OpenRouter API key is not set. Set Llm:ApiKey in config or OPENROUTER_API_KEY env var.");
            return;
        }
        if (string.IsNullOrWhiteSpace(openrouterUrl))
        {
            Logger.Error("OpenRouter URL is not set. Set Llm:OpenRouterUrl in config or OPENROUTER_API_URL env var.");
            return;
        }

        // 5. Creating LLM client
        ILlmClient llmClient = new OpenRouterLlmClient(llmApiKey, openrouterUrl, cfgModel);

        // 6. Instructions
        var instructionPath = Path.Combine(Path.GetDirectoryName(configPath) ?? Directory.GetCurrentDirectory(), "instructions.md");
        var instructionService = new InstructionService();
        var instructions = instructionService.LoadInstructions(instructionPath);
        Logger.Info($"Loaded instructions from {instructionPath}");

        // 7. OpenAPI
        var openApiService = new OpenApiService();
        Logger.Info($"Fetching OpenAPI from {openApiUrl}");
        var endpoints = await openApiService.ParseOpenApiAsync(openApiUrl, userName ?? string.Empty, userPassword ?? string.Empty);
        Logger.Info($"Parsed {endpoints.Count} endpoints from OpenAPI");

        // 8. Test case generation
        var generator = new TestCaseGeneratorService(llmClient, instructions);
        var allCases = new List<TestCase>();
        foreach (var endpoint in endpoints)
        {
            try
            {
                Logger.Info($"Generating test cases for {endpoint.Method} {endpoint.Path}");
                var cases = await generator.GenerateTestCasesAsync(endpoint);

                if (cases == null || cases.Count == 0)
                {
                    allCases.Add(new TestCase
                    {
                        Title = $"No test cases generated",
                        Body = "",
                        Endpoint = endpoint.Path,
                        Method = endpoint.Method,
                        Type = "Error",
                        Steps = new List<string> { "No test cases were generated for this endpoint." },
                        ExpectedResult = "Test generation failed or returned no results."
                    });
                    Logger.Warn($"No test cases generated for {endpoint.Method} {endpoint.Path}");
                }
                else
                {
                    allCases.AddRange(cases);
                    Logger.Info($"Generated {cases.Count} test case(s) for {endpoint.Method} {endpoint.Path}");
                }
            }
            catch (Exception ex)
            {
                allCases.Add(new TestCase
                {
                    Title = $"Error generating test cases",
                    Body = "",
                    Endpoint = endpoint.Path,
                    Method = endpoint.Method,
                    Type = "Error",
                    Steps = new List<string> { $"Exception: {ex.Message}" },
                    ExpectedResult = "Test generation failed due to exception."
                });
                Logger.Error($"Failed to generate for {endpoint.Path} {endpoint.Method}: {ex.Message}");
                Logger.Error($"StackTrace: {ex.StackTrace}");
            }
        }

        // 9. Export
        var exportService = new TestCaseExportService();
        exportService.ExportToCsv(allCases, options.OutputFile);
        Logger.Info($"Generated {allCases.Count} test case(s) in total. Saved to {options.OutputFile}");

        var excelPath = Path.ChangeExtension(options.OutputFile, ".xlsx");
        exportService.ExportToExcel(allCases, excelPath);
        Logger.Info($"Also saved Excel file: {excelPath}");
    }
}