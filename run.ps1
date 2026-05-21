param(
    [string]$ApiKey = $null,
    [string]$OpenApiUrl = $null,
    [string]$Output = $null,
    [switch]$Help
)

if ($Help) {
    Write-Host "Usage: .\run.ps1 [-ApiKey <key>] [-OpenApiUrl <url>] [-Output <file>]"
    Write-Host "Examples:"
    Write-Host "  .\run.ps1 -ApiKey 'sk-...'"
    Write-Host "  .\run.ps1 -Output mycases.csv"
    exit 0
}

$projDir = Join-Path $PSScriptRoot "ApiTestAssistantPortfolioProject"
$proj = Join-Path $projDir "ApiTestAssistantPortfolioProject.csproj"
$config = Join-Path $projDir "appsettings.json"

if (-not (Test-Path $proj)) {
    Write-Error "Project file not found: $proj"
    exit 1
}

# Set API key for session if provided
if ($ApiKey) {
    $env:OPENROUTER_API_KEY = $ApiKey
    Write-Host "OPENROUTER_API_KEY set for this session"
}

# Prepare output directory and file name
$testsDir = Join-Path $projDir "ApiTestAssistant.Tests"
if (-not (Test-Path $testsDir)) { New-Item -ItemType Directory -Path $testsDir | Out-Null }

# Generate unique file name if not provided
if (-not $Output) {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $Output = "Generated_test_cases_$timestamp.csv"
}
$outputPath = Join-Path $testsDir $Output

# Build argument list for dotnet run
$argsList = @()
if ($OpenApiUrl) { $argsList += "--openapi-url"; $argsList += $OpenApiUrl }
$argsList += "--config"; $argsList += $config
$argsList += "--output"; $argsList += $outputPath

$dotnetArgs = @("run","--project",$proj,"--") + $argsList

Write-Host "Running: dotnet $($dotnetArgs -join ' ')"
Write-Host "Output file will be: $outputPath"

$proc = Start-Process -FilePath dotnet -ArgumentList $dotnetArgs -NoNewWindow -Wait -PassThru
exit $proc.ExitCode