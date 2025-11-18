using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace oneKeyAi_win.Services
{
    public interface ITextResponse
    {
        string Content { get; set; }
        Dictionary<string, object>? Metadata { get; set; }
    }

    public class StandardTextResponse : ITextResponse
    {
        public string Content { get; set; } = string.Empty;
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public interface ILargeModelService
    {
        // Common method for text generation (the primary function)
        Task<ITextResponse> GenerateTextAsync(string model, string prompt, double temperature = 0.7, int maxTokens = 1000);

        // Configuration methods
        void SetApiKey(string apiKey);
        void SetBaseUrl(string baseUrl);
    }
}