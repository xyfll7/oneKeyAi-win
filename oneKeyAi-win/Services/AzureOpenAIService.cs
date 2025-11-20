using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace oneKeyAi_win.Services
{
    public sealed partial class AzureOpenAIService : ILargeModelService, IDisposable
    {
        private static readonly Lazy<AzureOpenAIService> _instance =
            new(() => new AzureOpenAIService());

        public static AzureOpenAIService Instance => _instance.Value;

        private readonly HttpClient _httpClient;
        private string _apiKey = "";
        private string _baseUrl = "";
        private string _deploymentName = "";
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public AzureOpenAIService() : this("", "", "")
        {
        }

        public AzureOpenAIService(string apiKey, string baseUrl, string deploymentName)
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };
            _apiKey = apiKey;
            _baseUrl = baseUrl;
            _deploymentName = deploymentName;
        }

        public void SetApiKey(string apiKey)
        {
            _apiKey = apiKey;
        }

        public void SetBaseUrl(string baseUrl)
        {
            _baseUrl = baseUrl;
        }

        public void SetDeploymentName(string deploymentName)
        {
            _deploymentName = deploymentName;
        }

        public async Task<AzureOpenAIResponse> ChatCompletionsAsync(List<Message> messages, double temperature = 0.7, int maxTokens = 1000)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Azure OpenAI API key is not set");

            if (string.IsNullOrWhiteSpace(_baseUrl))
                throw new InvalidOperationException("Azure OpenAI base URL is not set");

            if (string.IsNullOrWhiteSpace(_deploymentName))
                throw new InvalidOperationException("Azure OpenAI deployment name is not set");

            var request = new OpenAIRequest
            {
                Messages = messages,
                Temperature = temperature,
                MaxTokens = maxTokens
            };

            var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "oneKeyAi-win");

            HttpResponseMessage? response = null;
            try
            {
                var url = $"{_baseUrl}/openai/deployments/{_deploymentName}/chat/completions?api-version=2023-05-15";
                response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Azure OpenAI API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var azureResponse = JsonSerializer.Deserialize<AzureOpenAIResponse>(responseString);

                return azureResponse ?? throw new InvalidOperationException("Azure OpenAI 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 Azure OpenAI API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<AzureOpenAIResponse> CompletionsAsync(string prompt, double temperature = 0.7, int maxTokens = 1000)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Azure OpenAI API key is not set");

            if (string.IsNullOrWhiteSpace(_baseUrl))
                throw new InvalidOperationException("Azure OpenAI base URL is not set");

            if (string.IsNullOrWhiteSpace(_deploymentName))
                throw new InvalidOperationException("Azure OpenAI deployment name is not set");

            var request = new LegacyOpenAIRequest
            {
                Prompt = prompt,
                Temperature = temperature,
                MaxTokens = maxTokens
            };

            var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "oneKeyAi-win");

            HttpResponseMessage? response = null;
            try
            {
                var url = $"{_baseUrl}/openai/deployments/{_deploymentName}/completions?api-version=2023-05-15";
                response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Azure OpenAI API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var azureResponse = JsonSerializer.Deserialize<AzureOpenAIResponse>(responseString);

                return azureResponse ?? throw new InvalidOperationException("Azure OpenAI 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 Azure OpenAI API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<ITextResponse> GenerateTextAsync(string model, string prompt, double temperature = 0.7, int maxTokens = 1000)
        {
            var messages = new()
            {
                new Message { Role = "user", Content = prompt }
            };
            var azureResponse = await ChatCompletionsAsync(messages, temperature, maxTokens);

            // Extract the text content from the Azure OpenAI response
            string content = string.Empty;
            if (azureResponse.Choices != null)
            {
                foreach (var choice in azureResponse.Choices)
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
            if (azureResponse.Usage != null)
            {
                metadata["Usage"] = azureResponse.Usage;
            }
            if (!string.IsNullOrEmpty(azureResponse.Id))
            {
                metadata["Id"] = azureResponse.Id;
            }
            if (!string.IsNullOrEmpty(azureResponse.Model))
            {
                metadata["Model"] = azureResponse.Model;
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

    public class AzureOpenAIResponse
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
        public List<AzureChoice>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public AzureUsage? Usage { get; set; }
    }

    public class AzureChoice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public Message? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    public class AzureUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}