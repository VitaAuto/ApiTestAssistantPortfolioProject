using System;
using System.Collections.Generic;

namespace ApiTestAssistant.Core.Utils
{
    public static class GherkinFormatter
    {
        private static readonly string[] GherkinWords = { "Given", "When", "Then", "And", "But" };

        public static string FormatGherkinBlock(string block)
        {
            var lines = block.Split(new[] { ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var formatted = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                bool found = false;
                foreach (var word in GherkinWords)
                {
                    if (trimmed.StartsWith(word, StringComparison.OrdinalIgnoreCase))
                    {
                        formatted.Add($"*{word}*{trimmed.Substring(word.Length)}");
                        found = true;
                        break;
                    }
                }
                if (!found)
                    formatted.Add(trimmed);
            }
            return string.Join(Environment.NewLine, formatted);
        }
    }
}