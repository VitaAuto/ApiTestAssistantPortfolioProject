**ApiTestAssistant**

ApiTestAssistant is a C# console application designed to automatically generate API test cases (initially using Behavior-Driven Development) based on an OpenAPI specification. 
The application leverages a Large Language Model (LLM) via OpenRouter to create test scenarios, enabling fast and convenient generation of test cases for API validation.

**Key Features**
- Generates test cases (e.g., in Gherkin format) from OpenAPI specifications.
- Flexible configuration via environment variables, appsettings.json, or command-line options.
- Supports multiple output formats: CSV and Excel.
- Logs all errors and activity to ApiTestAssistant.log.
- Secure handling of secrets (API keys), with environment variables as the preferred method.

**Test Case Generation Instructions**
- All rules and best practices for generating API test cases are described in detail in the instructions.md file included in this project.
- instructions.md contains strict guidelines for generating BDD-style test cases for each API endpoint.
- It defines the required structure, format, and types of test cases (positive, negative, edge, etc.).
- The instructions ensure that all generated test cases are realistic, business-relevant, and technically meaningful.
- The file also provides an example and covers best practices for test data, Gherkin syntax, and handling of various scenarios (security, performance, pagination, etc.).
- These instructions are followed by the application to guarantee consistency and high quality of the generated test cases.
- If you want to understand or customize how test cases are generated, review and edit instructions.md.

**Setup and Run**
1. *Configure Your API Key*

You can provide your OpenRouter (LLM) API key in one of the following ways:

Recommended: Set an environment variable in your terminal session:
powershell

$env:OPENROUTER_API_KEY = "YOUR_API_KEY"

Alternative: Add it to ApiTestAssistantProject/appsettings.json under Llm:ApiKey
(Not recommended for secrets in production):

"Llm": {
  "ApiKey": "your_api_key_created_on_openrouter"
}

2. *Configure OpenAPI URL*

Set the OpenAPI URL in one of the following ways:

Environment variable:
powershell

$env:OPENAPI_URL = "https://petstore3.swagger.io/api/v3/openapi.json"
Or in appsettings.json:
json

"OpenApi": {
  "Url": "https://petstore3.swagger.io/api/v3/openapi.json"
}
You can also override this value via CLI options (see below).

3. *Run the Application*

A. The Short Way: PowerShell Script
From the repository root, simply run:
powershell

.\run.ps1

This will use the configuration from appsettings.json and environment variables.

You can also specify options:
powershell

.\run.ps1 -OpenApiUrl "https://petstore3.swagger.io/api/v3/openapi.json" -Output "mycases.csv" -ApiKey "sk-..."
Show help:
powershell

.\run.ps1 -Help
PowerShell script options:

-OpenApiUrl <url> — Override OpenAPI URL
-Output <file> — Output file path (CSV, Excel will be in the same folder)
-ApiKey <key> — LLM API key (or use OPENROUTER_API_KEY env var)
-Help — Show usage info

B. The Standard Way: dotnet CLI
Show help:
powershell

dotnet run --project .\ApiTestAssistantProject\ApiTestAssistantProject.csproj -- --help
Run with options:
powershell

dotnet run --project .\ApiTestAssistantProject\ApiTestAssistantProject.csproj -- --openapi-url https://petstore3.swagger.io/api/v3/openapi.json --output mycases.csv --apikey sk-...
CLI options:

  -c, --config <path>       Path to appsettings.json (default: look in cwd or project dir)
  -u, --openapi-url <url>   Override OpenAPI URL
  --apikey <key>            LLM API key (or use OPENROUTER_API_KEY env var)
  -o, --output <file>       Output file path (CSV, Excel will be in same folder with same name)
  -h, --help                Show this help
Note: The -- before CLI options is required to forward arguments to the app when using dotnet run.

4. *Output*

By default, the program writes a file named generated_test_cases_YYYYMMDD_HHMMSS.csv in the 'ApiTestAssistant.Tests' folder.
An Excel file with the same name will also be created in the same folder.

5. *Example appsettings.json*

{
  "OpenApi": {
    "Url": "https://petstore3.swagger.io/api/v3/openapi.json",
    "UserName": "your_username",
    "UserPassword": "your_password"
  },
  "Llm": {
    "Provider": "OpenRouter",
    "ApiKey": "your_api_key_created_on_openrouter",
    "OpenRouterUrl": "https://openrouter.ai/api/v1/chat/completions",
    "OpenRouterModel": "openrouter/free"
  }
}

6. *Notes*

- CLI/script options take precedence over config file and environment variables.
- For secrets (API keys, passwords), always prefer environment variables.
- All errors and logs are written to ApiTestAssistant.log in the working directory.
- For more details on options, run with --help (CLI) or -Help (PowerShell script).