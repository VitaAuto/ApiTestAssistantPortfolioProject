using System;
using System.Collections.Generic;
using System.Text.Json;
using ApiTestAssistant.Core.Models;

namespace ApiTestAssistant.Core.Parsers
{
    public class JsonTestCaseParser : ITestCaseParser
    {
        public List<TestCase> Parse(string text, ApiEndpoint endpoint)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var testCases = JsonSerializer.Deserialize<List<TestCase>>(text, options);

                if (testCases != null)
                {
                    foreach (var tc in testCases)
                    {
                        if (string.IsNullOrEmpty(tc.Endpoint)) tc.Endpoint = endpoint.Path;
                        if (string.IsNullOrEmpty(tc.Method)) tc.Method = endpoint.Method;
                    }

                    return testCases;
                }
                else
                {
                    return new List<TestCase>();
                }
            }
            catch
            {
                return new List<TestCase>();
            }
        }
    }
}