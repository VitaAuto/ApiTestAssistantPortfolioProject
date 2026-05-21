using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ApiTestAssistant.Core.Models;

namespace ApiTestAssistant.Core.Parsers
{
    public class MarkdownTestCaseParser : ITestCaseParser
    {
        public List<TestCase> Parse(string text, ApiEndpoint endpoint)
        {
            var testCases = new List<TestCase>();
            var matches = Regex.Matches(text, @"###\s*Test Case\s*(\d+):\s*(.+?)\n(.*?)(?=###|$)", RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                var title = match.Groups[2].Value.Trim();
                var bodySection = match.Groups[3].Value;

                var steps = new List<string>();
                var expected = "";
                var body = "";

                var stepMatch = Regex.Match(bodySection, @"Steps:\s*(.+?)\n", RegexOptions.Singleline);
                if (stepMatch.Success)
                    steps.AddRange(stepMatch.Groups[1].Value.Split(new[] { '\n', '-' }, StringSplitOptions.RemoveEmptyEntries));

                var expectedMatch = Regex.Match(bodySection, @"Expected Result:\s*(.+?)\n", RegexOptions.Singleline);
                if (expectedMatch.Success)
                    expected = expectedMatch.Groups[1].Value.Trim();

                var bodyMatch = Regex.Match(bodySection, @"Body:\s*(.+?)\n", RegexOptions.Singleline);
                if (bodyMatch.Success)
                    body = bodyMatch.Groups[1].Value.Trim();

                testCases.Add(new TestCase
                {
                    Title = title,
                    Body = body,
                    Endpoint = endpoint.Path,
                    Method = endpoint.Method,
                    Type = "Functional",
                    Steps = steps,
                    ExpectedResult = expected
                });
            }
            return testCases;
        }
    }
}