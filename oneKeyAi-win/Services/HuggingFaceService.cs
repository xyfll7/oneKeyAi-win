using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace oneKeyAi_win.Services
{
    public sealed partial class HuggingFaceService : ILargeModelService, IDisposable
    {
        private static readonly Lazy<HuggingFaceService> _instance =
            new(() => new HuggingFaceService());

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static HuggingFaceService Instance => _instance.Value;

        private readonly HttpClient _httpClient;
        private string _apiKey = "";
        private string _baseUrl = "https://api-inference.huggingface.co/models";

        public HuggingFaceService() : this("", "https://api-inference.huggingface.co/models")
        {
        }

        public HuggingFaceService(string apiKey, string baseUrl = "https://api-inference.huggingface.co/models")
        {
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(10) // Hugging Face can take longer for inference
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

        public async Task<List<HuggingFaceResponse>> TextGenerationAsync(string model, string prompt, double temperature = 0.7, int maxNewTokens = 1000, int topK = 50, double topP = 0.95, double repetitionPenalty = 1.0)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Hugging Face API key is not set");

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or empty", nameof(model));

            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));

            var parameters = new Dictionary<string, object>
            {
                ["temperature"] = temperature,
                ["max_new_tokens"] = maxNewTokens,
                ["top_k"] = topK,
                ["top_p"] = topP,
                ["repetition_penalty"] = repetitionPenalty,
                ["return_full_text"] = false // Only return generated text, not the original prompt
            };

            var request = new HuggingFaceRequest
            {
                Inputs = prompt,
                Parameters = parameters
            };

            var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "oneKeyAi-win");

            HttpResponseMessage? response = null;
            try
            {
                var url = $"{_baseUrl}/{model}";
                response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Hugging Face API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();

                // Hugging Face returns an array of results
                var huggingFaceResponse = JsonSerializer.Deserialize<List<HuggingFaceResponse>>(responseString);

                return huggingFaceResponse ?? throw new InvalidOperationException("Hugging Face 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 Hugging Face API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<List<HuggingFaceResponse>> QuestionAnsweringAsync(string model, string context, string question)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Hugging Face API key is not set");

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or empty", nameof(model));

            if (string.IsNullOrWhiteSpace(context))
                throw new ArgumentException("Context cannot be null or empty", nameof(context));

            if (string.IsNullOrWhiteSpace(question))
                throw new ArgumentException("Question cannot be null or empty", nameof(question));

            var request = new QuestionAnsweringRequest
            {
                Inputs = new QAInputs
                {
                    Context = context,
                    Question = question
                }
            };

            var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "oneKeyAi-win");

            HttpResponseMessage? response = null;
            try
            {
                var url = $"{_baseUrl}/{model}";
                response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Hugging Face API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();

                // For question answering, the response format might be different
                var huggingFaceResponse = JsonSerializer.Deserialize<List<HuggingFaceResponse>>(responseString);

                return huggingFaceResponse ?? throw new InvalidOperationException("Hugging Face 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 Hugging Face API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<List<HuggingFaceResponse>> SummarizationAsync(string model, string text, int minLength = 20, int maxLength = 1000)
        {
            if (string.IsNullOrWhiteSpace(_apiKey))
                throw new InvalidOperationException("Hugging Face API key is not set");

            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be null or empty", nameof(model));

            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text cannot be null or empty", nameof(text));

            var parameters = new Dictionary<string, object>
            {
                ["min_length"] = minLength,
                ["max_length"] = maxLength
            };

            var request = new HuggingFaceRequest
            {
                Inputs = text,
                Parameters = parameters
            };

            var json = JsonSerializer.Serialize(request, _jsonSerializerOptions);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "oneKeyAi-win");

            HttpResponseMessage? response = null;
            try
            {
                var url = $"{_baseUrl}/{model}";
                response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"Hugging Face API 错误: {response.StatusCode}\n{errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();

                var huggingFaceResponse = JsonSerializer.Deserialize<List<HuggingFaceResponse>>(responseString);

                return huggingFaceResponse ?? throw new InvalidOperationException("Hugging Face 返回空响应");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"调用 Hugging Face API 失败: {ex.Message}", ex);
            }
            finally
            {
                response?.Dispose();
            }
        }

        public async Task<ITextResponse> GenerateTextAsync(string model, string prompt, double temperature = 0.7, int maxTokens = 1000)
        {
            var huggingFaceResponses = await TextGenerationAsync(model, prompt, temperature, maxTokens);

            // Extract the text content from the first Hugging Face response
            string content = string.Empty;
            HuggingFaceResponse? firstResponse = huggingFaceResponses?.FirstOrDefault();

            if (firstResponse != null)
            {
                if (!string.IsNullOrEmpty(firstResponse.GeneratedText))
                {
                    content = firstResponse.GeneratedText;
                }
                else if (firstResponse.GeneratedTexts != null && firstResponse.GeneratedTexts.Count > 0)
                {
                    content = firstResponse.GeneratedTexts[0]; // Return first generated text
                }
                else if (!string.IsNullOrEmpty(firstResponse.Answer))
                {
                    content = firstResponse.Answer;
                }
            }

            // Create metadata dictionary with relevant information
            var metadata = new Dictionary<string, object>();
            if (firstResponse?.Score.HasValue == true)
            {
                metadata["Score"] = firstResponse.Score.Value;
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

    public class HuggingFaceRequest
    {
        public string Inputs { get; set; } = string.Empty;
        public Dictionary<string, object>? Parameters { get; set; }
        public Dictionary<string, object>? Options { get; set; }
    }

    public class QuestionAnsweringRequest
    {
        public QAInputs Inputs { get; set; } = new();
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class QAInputs
    {
        public string Context { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
    }

    public class HuggingFaceResponse
    {
        [JsonPropertyName("generated_text")]
        public string? GeneratedText { get; set; }

        [JsonPropertyName("generated_texts")] // Some models return multiple results
        public List<string>? GeneratedTexts { get; set; }

        [JsonPropertyName("score")]
        public double? Score { get; set; }

        [JsonPropertyName("answer")]
        public string? Answer { get; set; }

        [JsonPropertyName("start")]
        public int? Start { get; set; }

        [JsonPropertyName("end")]
        public int? End { get; set; }
    }
}