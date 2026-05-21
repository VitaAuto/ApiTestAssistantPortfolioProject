using System;
using System.Collections.Generic;
using ApiTestAssistant.Core.Models;

namespace ApiTestAssistant.Core.Parsers
{
    public class GherkinTestCaseParser : ITestCaseParser
    {
        public List<TestCase> Parse(string gherkinText, ApiEndpoint endpoint)
        {
            var lines = gherkinText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var testCases = new List<TestCase>();
            TestCase? current = null;
            var steps = new List<string>();
            var expected = new List<string>();
            var bodyLines = new List<string>();
            bool scenarioStarted = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("#"))
                {
                    bodyLines.Add(trimmed.TrimStart('#').Trim());
                }
                else if (trimmed.StartsWith("Scenario:", StringComparison.OrdinalIgnoreCase))
                {
                    if (current != null)
                    {
                        current.Steps = new List<string>(steps);
                        current.ExpectedResult = string.Join(" ", expected);
                        current.Body = string.Join(" ", bodyLines);
                        testCases.Add(current);
                    }
                    var scenarioTitle = trimmed.Substring("Scenario:".Length).Trim();
                    current = new TestCase
                    {
                        Title = scenarioTitle,
                        Body = "",
                        Endpoint = endpoint.Path,
                        Method = endpoint.Method,
                        Type = GetTestType(scenarioTitle),
                        Steps = new List<string>(),
                        ExpectedResult = ""
                    };
                    steps.Clear();
                    expected.Clear();
                    bodyLines.Clear();
                    scenarioStarted = true;
                }
                else if (scenarioStarted && !trimmed.StartsWith("Given", StringComparison.OrdinalIgnoreCase) &&
                         !trimmed.StartsWith("When", StringComparison.OrdinalIgnoreCase) &&
                         !trimmed.StartsWith("Then", StringComparison.OrdinalIgnoreCase) &&
                         !trimmed.StartsWith("And", StringComparison.OrdinalIgnoreCase) &&
                         !trimmed.StartsWith("But", StringComparison.OrdinalIgnoreCase))
                {
                    bodyLines.Add(trimmed);
                }
                else if (trimmed.StartsWith("Given", StringComparison.OrdinalIgnoreCase) ||
                         trimmed.StartsWith("When", StringComparison.OrdinalIgnoreCase) ||
                         trimmed.StartsWith("And", StringComparison.OrdinalIgnoreCase) ||
                         trimmed.StartsWith("But", StringComparison.OrdinalIgnoreCase))
                {
                    steps.Add(trimmed);
                }
                else if (trimmed.StartsWith("Then", StringComparison.OrdinalIgnoreCase))
                {
                    expected.Add(trimmed);
                }
            }
            if (current != null)
            {
                current.Steps = new List<string>(steps);
                current.ExpectedResult = string.Join(" ", expected);
                current.Body = string.Join(" ", bodyLines);
                testCases.Add(current);
            }
            return testCases;
        }

        private string GetTestType(string scenarioTitle)
        {
            var title = scenarioTitle.ToLowerInvariant();
            if (title.Contains("invalid") || title.Contains("error") || title.Contains("fail") ||
                title.Contains("unauthorized") || title.Contains("forbidden") || title.Contains("bad request") ||
                title.Contains("not found") || title.Contains("conflict"))
                return "Negative";
            if (title.Contains("boundary") || title.Contains("edge") || title.Contains("limit") ||
                title.Contains("maximum") || title.Contains("minimum"))
                return "Edge";
            if (title.Contains("success") || title.Contains("valid") || title.Contains("positive"))
                return "Positive";
            return "Functional";
        }
    }
}