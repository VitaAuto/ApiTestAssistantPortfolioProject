using System;
using System.Collections.Generic;
using System.Text;

namespace ApiTestAssistant.LLM
{
    public interface ILlmClient
    {
        Task<string> GenerateAsync(string prompt);
    }
}
