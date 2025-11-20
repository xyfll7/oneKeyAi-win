using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace oneKeyAi_win.Services
{
    public sealed partial class AnthropicService : ILargeModelService, IDisposable
    {
        private static readonly Lazy<AnthropicService> _instance =
            new(() => new AnthropicService());

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public static AnthropicService Instance => _instance.Value;

        private readonly HttpClient _httpClient;
        private string _apiKey = "";
        private string _baseUrl = "https://api.anthropic.com/v1";

        public AnthropicService() : this("", "https://api.anthropic.com/v1")
        {
        }

        public AnthropicService(string apiKey, string baseUrl = "https://api.anthropic.com/v1")
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

        public async Task<AnthropicResponse> MessagesAsync(string model, string prompt, List<MessageBlock>? messages = null, double temperature = 0.7, int maxTokens = 1000)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Anthropic API key is not set");

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or empty", nameof(model));

            var request = new AnthropicRequest
            {
                Model = model ?? string.Empty,
                MaxTokens = maxTokens,
                Temperature = temperature
            };

            // Add messages - if no messages provided, create from prompt
            if (messages != null && messages.Count > 0)
            {
                request.Messages = messages;
            }
            else
            {
                request.Messages = [new MessageBlock { Role = "user", Content = prompt }];
            }

            var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "oneKeyAi-win");

            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.PostAsync($"{_baseUrl}/messages", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Anthropic API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var anthropicResponse = JsonSerializer.Deserialize<AnthropicResponse>(responseString);

                return anthropicResponse ?? throw new InvalidOperationException("Anthropic 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 Anthropic API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<AnthropicResponse> LegacyCompletionAsync(string model, string prompt, double temperature = 0.7, int maxTokensToSample = 1000)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Anthropic API key is not set");

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or empty", nameof(model));

            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            var request = new LegacyAnthropicRequest
            {
                Model = model ?? string.Empty,
                Prompt = $"\n\nHuman: {prompt}\n\nAssistant:",
                MaxTokensToSample = maxTokensToSample,
                Temperature = temperature,
                StopSequences = ["\n\nHuman:"]
            };

            var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "oneKeyAi-win");

            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.PostAsync($"{_baseUrl}/complete", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Anthropic API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var anthropicResponse = JsonSerializer.Deserialize<AnthropicResponse>(responseString);

                return anthropicResponse ?? throw new InvalidOperationException("Anthropic 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 Anthropic API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<ITextResponse> GenerateTextAsync(string model, string prompt, double temperature = 0.7, int maxTokens = 1000)
        {
            var anthropicResponse = await MessagesAsync(model, prompt, null!, temperature, maxTokens);

            // Extract the text content from the Anthropic response
            string content = string.Empty;
            if (anthropicResponse.Content != null)
            {
                foreach (var contentBlock in anthropicResponse.Content)
                {
                    if (!string.IsNullOrEmpty(contentBlock?.Text))
                    {
                        content = contentBlock.Text;
                        break;
                    }
                }
            }

            // Create metadata dictionary with relevant information
            var metadata = new Dictionary<string, object>();
            if (anthropicResponse.Usage != null)
            {
                metadata["Usage"] = anthropicResponse.Usage;
            }
            if (!string.IsNullOrEmpty(anthropicResponse.Id))
            {
                metadata["Id"] = anthropicResponse.Id;
            }
            if (!string.IsNullOrEmpty(anthropicResponse.Model))
            {
                metadata["Model"] = anthropicResponse.Model;
            }
            if (!string.IsNullOrEmpty(anthropicResponse.Role))
            {
                metadata["Role"] = anthropicResponse.Role;
            }

            return new StandardTextResponse
            {
                Content = content,
                Metadata = metadata
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class AnthropicRequest
    {
        public string Model { get; set; } = string.Empty;
        public List<MessageBlock> Messages { get; set; } = [];
        public int MaxTokens { get; set; } = 1000;
        public double Temperature { get; set; } = 0.7;
        public List<string>? StopSequences { get; set; }
        public string? System { get; set; }
    }

    public class LegacyAnthropicRequest
    {
        public string Model { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public int MaxTokensToSample { get; set; } = 1000;
        public double Temperature { get; set; } = 0.7;
        public List<string>? StopSequences { get; set; }
        public string? Metadata { get; set; }
    }

    public class MessageBlock
    {
        public string Role { get; set; } = string.Empty; // "user" or "assistant"
        public string Content { get; set; } = string.Empty;
    }

    public class AnthropicResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("content")]
        public List<ContentBlock>? Content { get; set; }

        [JsonPropertyName("stop_reason")]
        public string? StopReason { get; set; }

        [JsonPropertyName("stop_sequence")]
        public string? StopSequence { get; set; }

        [JsonPropertyName("usage")]
        public UsageInfo? Usage { get; set; }
    }

    public class ContentBlock
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class UsageInfo
    {
        [JsonPropertyName("input_tokens")]
        public int InputTokens { get; set; }

        [JsonPropertyName("output_tokens")]
        public int OutputTokens { get; set; }
    }
}