using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace oneKeyAi_win.Services
{
    public sealed partial class GoogleAIService : ILargeModelService, IDisposable
    {
        private static readonly Lazy<GoogleAIService> _instance =
            new(() => new GoogleAIService());

        public static GoogleAIService Instance => _instance.Value;

        private readonly HttpClient _httpClient;
        private string _apiKey = "";
        private string _baseUrl = "https://generativelanguage.googleapis.com/v1beta";
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GoogleAIService() : this("", "https://generativelanguage.googleapis.com/v1beta")
        {
        }

        public GoogleAIService(string apiKey, string baseUrl = "https://generativelanguage.googleapis.com/v1beta")
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

        public async Task<GoogleAIResponse> GenerateContentAsync(string model, string prompt, double temperature = 0.7, int maxOutputTokens = 1000)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Google AI API key is not set");

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or empty", nameof(model));

            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            var request = new GoogleAIRequest
            {
                Contents = new List<Content>
                {
                    new Content
                    {
                        Parts =
                        {
                            new Part { Text = prompt }
                        }
                    }
                },
                GenerationConfig = new GenerationConfig
                {
                    Temperature = temperature,
                    MaxOutputTokens = maxOutputTokens
                }
            };

            var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;
            try
            {
                var url = $"{_baseUrl}/models/{model}:generateContent?key={_apiKey}";
                response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Google AI API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var googleResponse = JsonSerializer.Deserialize<GoogleAIResponse>(responseString);

                return googleResponse ?? throw new InvalidOperationException("Google AI 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 Google AI API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<GoogleAIResponse> GenerateContentStreamAsync(string model, string prompt, double temperature = 0.7, int maxOutputTokens = 1000)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Google AI API key is not set");

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or empty", nameof(model));

            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            var request = new GoogleAIRequest
            {
                Contents = new List<Content>
                {
                    new Content
                    {
                        Parts =
                        {
                            new Part { Text = prompt }
                        }
                    }
                },
                GenerationConfig = new GenerationConfig
                {
                    Temperature = temperature,
                    MaxOutputTokens = maxOutputTokens
                }
            };

            var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage? response = null;
            try
            {
                var url = $"{_baseUrl}/models/{model}:streamGenerateContent?key={_apiKey}&alt=sse";
                response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Google AI API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var googleResponse = JsonSerializer.Deserialize<GoogleAIResponse>(responseString);

                return googleResponse ?? throw new InvalidOperationException("Google AI 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 Google AI API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<ITextResponse> GenerateTextAsync(string model, string prompt, double temperature = 0.7, int maxTokens = 1000)
        {
            var googleResponse = await GenerateContentAsync(model, prompt, temperature, maxTokens);

            // Extract the text content from the Google AI response
            string content = string.Empty;
            if (googleResponse.Candidates != null)
            {
                foreach (var candidate in googleResponse.Candidates)
                {
                    if (candidate?.Content?.Parts != null)
                    {
                        foreach (var part in candidate.Content.Parts)
                        {
                            if (!string.IsNullOrEmpty(part?.Text))
                            {
                                content = part.Text;
                                break;
                            }
                        }
                        if (!string.IsNullOrEmpty(content))
                            break;
                    }
                }
            }

            // Create metadata dictionary with relevant information
            var metadata = new Dictionary<string, object>();
            if (googleResponse.UsageMetadata != null)
            {
                metadata["UsageMetadata"] = googleResponse.UsageMetadata;
            }
            if (!string.IsNullOrEmpty(googleResponse.ModelVersion))
            {
                metadata["ModelVersion"] = googleResponse.ModelVersion;
            }
            if (!string.IsNullOrEmpty(googleResponse.ResponseId))
            {
                metadata["ResponseId"] = googleResponse.ResponseId;
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

    public class GoogleAIRequest
    {
        public List<Content>? Contents { get; set; }
        public GenerationConfig? GenerationConfig { get; set; }
        public SafetySettings? SafetySettings { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public List<Part> Parts { get; set; } = new();
        [JsonPropertyName("role")]
        public string? Role { get; set; }  // 添加这个属性
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class GenerationConfig
    {
        public double? Temperature { get; set; }
        public int? MaxOutputTokens { get; set; }
        public double? TopP { get; set; }
        public int? TopK { get; set; }
    }

    public class SafetySettings
    {
        public string? Category { get; set; }
        public string? Threshold { get; set; }
    }

    public class GoogleAIResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate>? Candidates { get; set; }

        [JsonPropertyName("promptFeedback")]
        public PromptFeedback? PromptFeedback { get; set; }

        [JsonPropertyName("usageMetadata")]
        public UsageMetadata? UsageMetadata { get; set; }

        [JsonPropertyName("modelVersion")]
        public string? ModelVersion { get; set; }

        [JsonPropertyName("responseId")]
        public string? ResponseId { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string? FinishReason { get; set; }

        [JsonPropertyName("index")]
        public int? Index { get; set; }

        [JsonPropertyName("safetyRatings")]
        public List<SafetyRating>? SafetyRatings { get; set; }

        [JsonPropertyName("citationMetadata")]
        public CitationMetadata? CitationMetadata { get; set; }
    }

    public class PromptFeedback
    {
        [JsonPropertyName("safetyRatings")]
        public List<SafetyRating>? SafetyRatings { get; set; }
    }

    public class SafetyRating
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("probability")]
        public string? Probability { get; set; }

        [JsonPropertyName("blocked")]
        public bool? Blocked { get; set; }
    }

    public class CitationMetadata
    {
        [JsonPropertyName("citations")]
        public List<Citation>? Citations { get; set; }
    }

    public class UsageMetadata
    {
        [JsonPropertyName("promptTokenCount")]
        public int? PromptTokenCount { get; set; }

        [JsonPropertyName("candidatesTokenCount")]
        public int? CandidatesTokenCount { get; set; }

        [JsonPropertyName("totalTokenCount")]
        public int? TotalTokenCount { get; set; }

        [JsonPropertyName("promptTokensDetails")]
        public List<PromptTokenDetails>? PromptTokensDetails { get; set; }

        [JsonPropertyName("thoughtsTokenCount")]
        public int? ThoughtsTokenCount { get; set; }
    }

    public class PromptTokenDetails
    {
        [JsonPropertyName("modality")]
        public string? Modality { get; set; }

        [JsonPropertyName("tokenCount")]
        public int? TokenCount { get; set; }
    }

    public class Citation
    {
        [JsonPropertyName("startIndex")]
        public int? StartIndex { get; set; }

        [JsonPropertyName("endIndex")]
        public int? EndIndex { get; set; }

        [JsonPropertyName("uri")]
        public string? Uri { get; set; }

        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("license")]
        public string? License { get; set; }

        [JsonPropertyName("publicationDate")]
        public string? PublicationDate { get; set; }
    }
}