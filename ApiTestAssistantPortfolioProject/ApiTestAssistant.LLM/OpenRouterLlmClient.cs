using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ApiTestAssistant.Core.Utils;

namespace ApiTestAssistant.LLM
{
    public class OpenRouterLlmClient : ILlmClient, IDisposable
    {
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly string _model;
        private readonly HttpClient _http;

        public OpenRouterLlmClient(string apiKey, string apiUrl, string? model = null)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
            _model = !string.IsNullOrEmpty(model) ? model : "openrouter/free";

            var handler = new HttpClientHandler { UseProxy = true };
            _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(90) };

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                Common.Logger.Error("OpenRouter API key is not set. Provide it via OPENROUTER_API_KEY env var or Llm:ApiKey in config.");
                throw new InvalidOperationException("OpenRouter API key is not set.");
            }

            var payload = new
            {
                model = _model,
                messages = new[] {
                    new { role = "system", content = "You are a helpful assistant for generating API test cases." },
                    new { role = "user", content = prompt }
                },
                stream = false
            };

            var json = JsonSerializer.Serialize(payload);

            const int maxAttempts = 5;
            const int delayMs = 3000;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    using var content = new StringContent(json, Encoding.UTF8, "application/json");

                    Common.Logger.Info($"Calling OpenRouter API at {_apiUrl} with model {_model} (attempt {attempt})");

                    using var resp = await _http.PostAsync(_apiUrl, content);
                    var respText = await resp.Content.ReadAsStringAsync();
                    if (!resp.IsSuccessStatusCode)
                    {
                        Common.Logger.Error($"OpenRouter API returned {(int)resp.StatusCode}: {respText}");
                        throw new Exception($"OpenRouter API returned {(int)resp.StatusCode}: {respText}");
                    }

                    try
                    {
                        using var doc = JsonDocument.Parse(respText);
                        var root = doc.RootElement;
                        if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                        {
                            var message = choices[0].GetProperty("message").GetProperty("content").GetString();
                            return message ?? string.Empty;
                        }

                        if (root.TryGetProperty("message", out var msg) && msg.TryGetProperty("content", out var contentEl))
                        {
                            return contentEl.GetString() ?? string.Empty;
                        }

                        return respText;
                    }
                    catch (JsonException)
                    {
                        return respText;
                    }
                }
                catch (Exception ex)
                {
                    Common.Logger.Error($"Attempt {attempt} failed: {ex.Message}");
                    if (attempt == maxAttempts)
                        throw;
                    await Task.Delay(delayMs);
                }
            }

            throw new Exception("All retry attempts failed.");
        }

        public void Dispose()
        {
            _http?.Dispose();
        }
    }
}