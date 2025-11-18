using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace oneKeyAi_win.Services
{
    public sealed partial class OpenAIService : ILargeModelService, IDisposable
    {
        private static readonly Lazy<OpenAIService> _instance =
            new(() => new OpenAIService());

        public static OpenAIService Instance => _instance.Value;

        private readonly HttpClient _httpClient;
        private string _apiKey = "";
        private string _baseUrl = "https://api.openai.com/v1";

        public OpenAIService() : this("", "https://api.openai.com/v1")
        {
        }

        public OpenAIService(string apiKey, string baseUrl = "https://api.openai.com/v1")
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

        public async Task<OpenAIResponse> ChatCompletionsAsync(string model, string prompt, List<Message>? messages = null, double temperature = 0.7, int maxTokens = 1000)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("OpenAI API key is not set");

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or empty", nameof(model));

            var request = new OpenAIRequest
            {
                Model = model ?? string.Empty,
                Temperature = temperature,
                MaxTokens = maxTokens
            };

            // Add messages - if no messages provided, create from prompt
            if (messages != null && messages.Count > 0)
            {
                request.Messages = messages;
            }
            else
            {
                request.Messages = new List<Message>
                {
                    new Message { Role = "user", Content = prompt }
                };
            }

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "oneKeyAi-win");

            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.PostAsync($"{_baseUrl}/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"OpenAI API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseString);

                return openAIResponse ?? throw new InvalidOperationException("OpenAI 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 OpenAI API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<OpenAIResponse> CompletionsAsync(string model, string prompt, double temperature = 0.7, int maxTokens = 1000)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("OpenAI API key is not set");

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or empty", nameof(model));

            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            var request = new LegacyOpenAIRequest
            {
                Model = model ?? string.Empty,
                Prompt = prompt ?? string.Empty,
                Temperature = temperature,
                MaxTokens = maxTokens,
                Stop = new List<string> { "\n\n" }
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "oneKeyAi-win");

            HttpResponseMessage? response = null;
            try
            {
                response = await _httpClient.PostAsync($"{_baseUrl}/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"OpenAI API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var openAIResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseString);

                return openAIResponse ?? throw new InvalidOperationException("OpenAI 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 OpenAI API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<ITextResponse> GenerateTextAsync(string model, string prompt, double temperature = 0.7, int maxTokens = 1000)
        {
            var openaiResponse = await ChatCompletionsAsync(model, prompt, null!, temperature, maxTokens);

            // Extract the text content from the OpenAI response
            string content = string.Empty;
            if (openaiResponse.Choices != null)
            {
                foreach (var choice in openaiResponse.Choices)
                {
                    if (!string.IsNullOrEmpty(choice?.Message?.Content))
                    {
                        content = choice.Message.Content;
                        break;
                    }
                }
            }

            // Create metadata dictionary with relevant information
            var metadata = new Dictionary<string, object>();
            if (openaiResponse.Usage != null)
            {
                metadata["Usage"] = openaiResponse.Usage;
            }
            if (!string.IsNullOrEmpty(openaiResponse.Id))
            {
                metadata["Id"] = openaiResponse.Id;
            }
            if (!string.IsNullOrEmpty(openaiResponse.Model))
            {
                metadata["Model"] = openaiResponse.Model;
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

    public class OpenAIRequest
    {
        public string Model { get; set; } = string.Empty;
        public List<Message> Messages { get; set; } = new List<Message>();
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 1000;
        public List<string>? Stop { get; set; }
        public double FrequencyPenalty { get; set; } = 0;
        public double PresencePenalty { get; set; } = 0;
        public Dictionary<string, object>? Functions { get; set; }
        public string? FunctionCall { get; set; }
    }

    public class Message
    {
        public string Role { get; set; } = string.Empty; // "system", "user", "assistant"
        public string Content { get; set; } = string.Empty;
    }

    public class LegacyOpenAIRequest
    {
        public string Model { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public double Temperature { get; set; } = 0.7;
        public int MaxTokens { get; set; } = 1000;
        public double FrequencyPenalty { get; set; } = 0;
        public double PresencePenalty { get; set; } = 0;
        public List<string>? Stop { get; set; }
    }

    public class OpenAIResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("created")]
        public long Created { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }
    }

    public class Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public Message? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    public class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}