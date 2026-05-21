using System;
using System.IO;
using System.Threading;

namespace ApiTestAssistant.Common
{
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static string? _logFilePath;
        private static bool _initialized = false;

        public static void Init(string? directory = null)
        {
            lock (_lock)
            {
                if (_initialized) return;
                try
                {
                    var dir = string.IsNullOrEmpty(directory) ? AppContext.BaseDirectory : directory;
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    _logFilePath = Path.Combine(dir, "ApiTestAssistant.log");
                    _initialized = true;
                    Info($"Logger initialized. LogFile={_logFilePath}");
                }
                catch
                {
                    _initialized = true;
                }
            }
        }

        public static void Info(string message) => Log("INFO", message);
        public static void Warn(string message) => Log("WARN", message);
        public static void Error(string message) => Log("ERROR", message);

        private static void Log(string level, string message)
        {
            var text = $"[{DateTime.UtcNow:O}] {level}: {message}";
            lock (_lock)
            {
                try
                {
                    Console.WriteLine(text);
                    if (!string.IsNullOrEmpty(_logFilePath))
                    {
                        File.AppendAllText(_logFilePath, text + Environment.NewLine);
                    }
                }
                catch
                {
                }
            }
        }
    }
}
