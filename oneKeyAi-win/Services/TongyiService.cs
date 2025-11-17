using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace oneKeyAi_win.Services
{
    public sealed partial class TongyiService : ILargeModelService, IDisposable
    {
        private static readonly Lazy<TongyiService> _instance =
            new(() => new TongyiService());

        public static TongyiService Instance => _instance.Value;

        private readonly HttpClient _httpClient;
        private string _apiKey = "";
        private string _baseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1";

        public TongyiService() : this("", "https://dashscope.aliyuncs.com/compatible-mode/v1")
        {
        }

        public TongyiService(string apiKey, string baseUrl = "https://dashscope.aliyuncs.com/compatible-mode/v1")
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            _apiKey = apiKey;
            _baseUrl = baseUrl;
        }

        public void SetApiKey(string apiKey)
        {
            _apiKey = apiKey;
        }

        public void SetBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public async Task<TongyiResponse> ChatCompletionsAsync(string model, string prompt, List<TongyiMessage> messages = null, double temperature = 0.7, int maxTokens = 1000)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Tongyi API key is not set");

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or empty", nameof(model));

            // Use OpenAI-compatible format for messages
            List<TongyiMessage> messageList;
            if (messages != null && messages.Count > 0)
            {
                messageList = messages;
            }
            else
            {
                messageList = new List<TongyiMessage>
                {
                    new TongyiMessage { Role = "user", Content = prompt }
                };
            }

            // Create a request compatible with OpenAI format
            var request = new
            {
                model = model,
                messages = messageList,
                temperature = temperature,
                max_tokens = maxTokens
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "oneKeyAi-win");
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

            HttpResponseMessage? response = null;
            try
            {
                var url = $"{_baseUrl}/chat/completions"; // Use compatible mode endpoint
                response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Tongyi API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var tongyiResponse = JsonSerializer.Deserialize<TongyiResponse>(responseString);

                return tongyiResponse ?? throw new InvalidOperationException("Tongyi 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 Tongyi API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<TongyiResponse> TextEmbeddingAsync(string model, List<string> texts)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Tongyi API key is not set");

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or empty", nameof(model));

            if (texts == null || texts.Count == 0)
                throw new ArgumentException("Texts cannot be null or empty", nameof(texts));

            // Create a request compatible with OpenAI format
            var request = new
            {
                model = model,
                input = texts
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "oneKeyAi-win");
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

            HttpResponseMessage? response = null;
            try
            {
                var url = $"{_baseUrl}/embeddings"; // Use compatible mode endpoint
                response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Tongyi Embedding API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var tongyiResponse = JsonSerializer.Deserialize<TongyiResponse>(responseString);

                return tongyiResponse ?? throw new InvalidOperationException("Tongyi Embedding 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 Tongyi Embedding API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<TongyiResponse> ImageSynthesisAsync(string model, string prompt, string n, string size = "1024x1024")
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Tongyi API key is not set");

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or empty", nameof(model));

            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            // For image synthesis, we might need to keep using the original API as it might not be available in compatible mode
            var request = new TongyiImageRequest
            {
                Model = model,
                Input = new TongyiImageInput
                {
                    Prompt = prompt
                },
                Parameters = new TongyiImageParameters
                {
                    N = n,
                    Size = size
                }
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "oneKeyAi-win");
            _httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

            HttpResponseMessage? response = null;
            try
            {
                // Using the original API for image synthesis as it might not support compatible mode
                var url = $"{_baseUrl.Replace("/compatible-mode/v1", "/api/v1")}/services/aigc/text-to-image/image-synthesis";
                response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Tongyi Image Synthesis API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var tongyiResponse = JsonSerializer.Deserialize<TongyiResponse>(responseString);

                return tongyiResponse ?? throw new InvalidOperationException("Tongyi Image Synthesis 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 Tongyi Image Synthesis API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<object> GenerateTextAsync(string model, string prompt, double temperature = 0.7, int maxTokens = 1000)
        {
            return await ChatCompletionsAsync(model, prompt, null, temperature, maxTokens);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class TongyiRequest
    {
        public string Model { get; set; } = string.Empty;
        public TongyiInput Input { get; set; } = new TongyiInput();
        public TongyiParameters Parameters { get; set; } = new TongyiParameters();
    }

    public class TongyiInput
    {
        public List<TongyiMessage> Messages { get; set; } = new List<TongyiMessage>();
    }

    public class TongyiMessage
    {
        public string Role { get; set; } = string.Empty; // "system", "user", "assistant"
        public string Content { get; set; } = string.Empty;
    }

    public class TongyiParameters
    {
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 1000;
        public double TopP { get; set; } = 0.8;
        public int? Seed { get; set; }
    }

    public class TongyiEmbeddingRequest
    {
        public string Model { get; set; } = string.Empty;
        public TongyiEmbeddingInput Input { get; set; } = new TongyiEmbeddingInput();
        public TongyiEmbeddingParameters Parameters { get; set; } = new TongyiEmbeddingParameters();
    }

    public class TongyiEmbeddingInput
    {
        public List<string> Texts { get; set; } = new List<string>();
    }

    public class TongyiEmbeddingParameters
    {
        public string? TextType { get; set; } // "query" or "document"
    }

    public class TongyiImageRequest
    {
        public string Model { get; set; } = string.Empty;
        public TongyiImageInput Input { get; set; } = new TongyiImageInput();
        public TongyiImageParameters Parameters { get; set; } = new TongyiImageParameters();
    }

    public class TongyiImageInput
    {
        public string Prompt { get; set; } = string.Empty;
    }

    public class TongyiImageParameters
    {
        public string? N { get; set; }
        public string? Size { get; set; }
        public string? Style { get; set; }
        public string? RefImage { get; set; }
        public string? RefMode { get; set; }
    }

    public class TongyiResponse
    {
        [JsonPropertyName("output")]
        public TongyiOutput? Output { get; set; }

        [JsonPropertyName("usage")]
        public TongyiUsage? Usage { get; set; }

        [JsonPropertyName("request_id")]
        public string? RequestId { get; set; }

        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    public class TongyiOutput
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("choices")]
        public List<TongyiChoice>? Choices { get; set; }

        [JsonPropertyName("embeddings")]
        public List<TongyiEmbedding>? Embeddings { get; set; }

        [JsonPropertyName("data")]
        public List<TongyiImageData>? Data { get; set; }
    }

    public class TongyiChoice
    {
        [JsonPropertyName("message")]
        public TongyiMessage? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    public class TongyiEmbedding
    {
        [JsonPropertyName("embedding")]
        public List<double>? Embedding { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }
    }

    public class TongyiImageData
    {
        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    public class TongyiUsage
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}