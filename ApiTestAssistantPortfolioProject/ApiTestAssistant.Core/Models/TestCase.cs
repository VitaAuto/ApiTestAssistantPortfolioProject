using System;
using System.Collections.Generic;
using System.Text;

namespace ApiTestAssistant.Core.Models
{
    public class TestCase
    {
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public List<string> Steps { get; set; } = new List<string>();
        public string ExpectedResult { get; set; } = string.Empty;
    }
}
