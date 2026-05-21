using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiTestAssistant.Core.Utils
{
    public static class FileExportUtils
    {
        public static string EscapeCsv(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;

            var sb = new StringBuilder();
            foreach (var c in s)
            {
                if (!char.IsControl(c) || c == '\n' || c == '\r')
                    sb.Append(c);
            }
            var clean = sb.ToString();

            if (clean.Contains(",") || clean.Contains("\n") || clean.Contains("\""))
            {
                clean = clean.Replace("\"", "\"\"");
                return $"\"{clean}\"";
            }
            return clean;
        }

        public static string CleanBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body)) return string.Empty;

            var match = Regex.Match(body, @"(\{[\s\S]*?\}|\[[\s\S]*?\])");
            if (match.Success)
                return match.Value.Trim();

            return string.Empty;
        }
    }
}
