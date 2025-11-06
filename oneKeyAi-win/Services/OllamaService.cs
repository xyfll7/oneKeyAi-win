using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace oneKeyAi_win.Services
{
    public sealed partial class OllamaService : IDisposable
    {
        private static readonly Lazy<OllamaService> _instance =
            new(() => new OllamaService("http://127.0.0.1:11434")); // Create singleton with default URL

        public static OllamaService Instance => _instance.Value;

        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        // Default constructor for DI container, uses default URL
        public OllamaService() : this("http://127.0.0.1:11434")
        {
        }

        // Constructor that accepts base URL
        public OllamaService(string baseUrl)
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(10)
            };
            _baseUrl = baseUrl;
        }

        public async Task<OllamaResponse> GenerateAsync(string model, bool think, string prompt)
        {
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or empty", nameof(model));

            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            var request = new OllamaRequest
            {
                Model = model,
                Prompt = prompt,
                Stream = false,
                Think = think
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Ollama API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(responseString);

                return ollamaResponse ?? throw new InvalidOperationException("Ollama 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 Ollama API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class OllamaRequest
    {
        public string? Model { get; set; }
        public string? Prompt { get; set; }
        public bool Stream { get; set; }
        public bool Think { get; set; }
    }

    public class OllamaResponse
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("response")]
        public string? Response { get; set; }

        [JsonPropertyName("done")]
        public bool Done { get; set; }

        [JsonPropertyName("done_reason")]
        public string? DoneReason { get; set; }

        [JsonPropertyName("total_duration")]
        public long TotalDuration { get; set; }

        [JsonPropertyName("load_duration")]
        public long LoadDuration { get; set; }

        [JsonPropertyName("prompt_eval_count")]
        public int PromptEvalCount { get; set; }

        [JsonPropertyName("prompt_eval_duration")]
        public long PromptEvalDuration { get; set; }

        [JsonPropertyName("eval_count")]
        public int EvalCount { get; set; }

        [JsonPropertyName("eval_duration")]
        public long EvalDuration { get; set; }

        [JsonPropertyName("context")]
        public int[]? Context { get; set; }
    }

}