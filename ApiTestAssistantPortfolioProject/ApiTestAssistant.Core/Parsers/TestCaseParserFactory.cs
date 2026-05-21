using System.Collections.Generic;
using System.Linq;
using ApiTestAssistant.Core.Models;

namespace ApiTestAssistant.Core.Parsers
{
    public class TestCaseParserFactory
    {
        private readonly List<ITestCaseParser> _parsers;

        public TestCaseParserFactory()
        {
            _parsers = new List<ITestCaseParser>
            {
                new GherkinTestCaseParser(),
                new MarkdownTestCaseParser(),
                new JsonTestCaseParser()
            };
        }

        public ITestCaseParser GetParser(string text)
        {
            if (text.Contains("Scenario:")) return _parsers.OfType<GherkinTestCaseParser>().First();
            if (text.TrimStart().StartsWith("{") || text.TrimStart().StartsWith("[")) return _parsers.OfType<JsonTestCaseParser>().First();
            if (text.Contains("### Test Case")) return _parsers.OfType<MarkdownTestCaseParser>().First();
            return _parsers.First();
        }
    }
}