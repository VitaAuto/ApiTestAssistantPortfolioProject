using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ApiTestAssistant.Core.Models;

namespace ApiTestAssistant.Core.Services
{
    public class OpenApiService
    {
        public OpenApiService()
        {
        }
        public async Task<List<ApiEndpoint>> ParseOpenApiAsync(string url, string username, string userPassword)
        {
            var result = new List<ApiEndpoint>();
            if (string.IsNullOrWhiteSpace(url) || url == "")
            {
                return result;
            }

            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(userPassword))
                {
                    try
                    {
                        var uri = new Uri(url);
                        var baseUri = new Uri(uri.GetLeftPart(UriPartial.Authority));
                        var loginPath = "/api/Auth/login";
                        var loginUrl = new Uri(baseUri, loginPath).ToString();
                        Common.Logger.Info($"Attempting login to {loginUrl} to obtain token");
                        var loginPayload = new { username = username, password = userPassword };
                        var loginJson = JsonSerializer.Serialize(loginPayload);
                        using var loginContent = new StringContent(loginJson, System.Text.Encoding.UTF8, "application/json");
                        using var loginResp = await http.PostAsync(loginUrl, loginContent);
                        var loginText = await loginResp.Content.ReadAsStringAsync();
                        if (loginResp.IsSuccessStatusCode)
                        {
                            Common.Logger.Info($"Login succeeded. Parsing token from response");
                            try
                            {
                                using var doc = JsonDocument.Parse(loginText);
                                var root = doc.RootElement;
                                string? token = null;
                                if (root.ValueKind == JsonValueKind.Object)
                                {
                                    if (root.TryGetProperty("token", out var t)) token = t.GetString();
                                    else if (root.TryGetProperty("access_token", out var at)) token = at.GetString();
                                    else if (root.TryGetProperty("jwt", out var jt)) token = jt.GetString();
                                    else if (root.TryGetProperty("data", out var data) && data.ValueKind == JsonValueKind.Object && data.TryGetProperty("token", out var dt)) token = dt.GetString();
                                }

                                if (!string.IsNullOrEmpty(token))
                                {
                                    Common.Logger.Info($"Obtained token from login response (length={token.Length})");
                                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                                }
                                else
                                {
                                    Common.Logger.Warn($"Login succeeded but token not found in response: {loginText}");
                                }
                            }
                            catch (JsonException)
                            {
                                Common.Logger.Warn($"Failed to parse login response as JSON: {loginText}");
                            }
                        }
                        else
                        {
                            Common.Logger.Error($"Login failed { (int)loginResp.StatusCode }: {loginText }");
                        }
                    }
                    catch (Exception ex)
                    {
                        Common.Logger.Error($"Login flow failed: {ex.Message}");
                    }
                }

                Common.Logger.Info($"Requesting OpenAPI document from {url}");
                var resp = await http.GetAsync(url);
                resp.EnsureSuccessStatusCode();
                var stream = await resp.Content.ReadAsStreamAsync();

                var reader = new Microsoft.OpenApi.Readers.OpenApiStreamReader();
                var openApiDoc = reader.Read(stream, out var diagnostic);

                foreach (var path in openApiDoc.Paths)
                {
                    foreach (var op in path.Value.Operations)
                    {
                        var endpoint = new ApiEndpoint
                        {
                            Path = path.Key,
                            Method = op.Key.ToString(),
                            Summary = op.Value.Summary ?? string.Empty,
                            Parameters = new List<string>(),
                            RequestSchema = op.Value.RequestBody?.ToString() ?? string.Empty,
                            ResponseSchema = op.Value.Responses?.ToString() ?? string.Empty
                        };

                        if (op.Value.Parameters != null)
                        {
                            foreach (var p in op.Value.Parameters)
                            {
                                endpoint.Parameters.Add(p.Name);
                            }
                        }

                        result.Add(endpoint);
                    }
                }
            }
            catch (Exception ex)
            {
                Common.Logger.Error($"OpenAPI parse failed: {ex.Message}");
                Common.Logger.Error($"StackTrace: {ex.StackTrace}");
            }

            return result;
        }
    }
}
