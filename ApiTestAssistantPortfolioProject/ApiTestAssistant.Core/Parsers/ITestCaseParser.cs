using System.Collections.Generic;
using ApiTestAssistant.Core.Models;

namespace ApiTestAssistant.Core.Parsers
{
    public interface ITestCaseParser
    {
        List<TestCase> Parse(string text, ApiEndpoint endpoint);
    }
}