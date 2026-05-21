using System;
using System.Collections.Generic;
using System.Text;

namespace ApiTestAssistant.Core.Models;

public class ApiEndpoint
{
    public string Path { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<string> Parameters { get; set; } = new List<string>();
    public string RequestSchema { get; set; } = string.Empty;
    public string ResponseSchema { get; set; } = string.Empty;
}
