using System.Collections.Generic;
using ApiTestAssistant.Core.Models;
using ApiTestAssistant.LLM;
using ApiTestAssistant.Core.Parsers;

namespace ApiTestAssistant.Core.Services
{
    public class TestCaseGeneratorService
    {
        private readonly ILlmClient _llmClient;
        private readonly InstructionSet _instructions;
        private readonly TestCaseParserFactory _parserFactory;

        public TestCaseGeneratorService(ILlmClient llmClient, InstructionSet instructions)
        {
            _llmClient = llmClient;
            _instructions = instructions;
            _parserFactory = new TestCaseParserFactory();
        }

        public async Task<List<TestCase>> GenerateTestCasesAsync(ApiEndpoint endpoint)
        {
            var prompt = $"{_instructions.RawInstructions}\n\nEndpoint: {endpoint.Path}\nMethod: {endpoint.Method}\nSummary: {endpoint.Summary}\n";
            var response = await _llmClient.GenerateAsync(prompt);

            var parser = _parserFactory.GetParser(response);
            var testCases = parser.Parse(response, endpoint);

            return testCases;
        }
    }
}