using System;

namespace oneKeyAi_win.Services
{
    public static class LargeModelServiceFactory
    {
        public static object GetService(ModelProvider provider)
        {
            return provider switch
            {
                ModelProvider.OpenAI => OpenAIService.Instance,
                ModelProvider.AzureOpenAI => AzureOpenAIService.Instance,
                ModelProvider.GoogleAI => GoogleAIService.Instance,
                ModelProvider.Anthropic => AnthropicService.Instance,
                ModelProvider.HuggingFace => HuggingFaceService.Instance,
                ModelProvider.Ollama => OllamaService.Instance,
                ModelProvider.Tongyi => TongyiService.Instance,
                _ => throw new ArgumentException($"Unsupported model provider: {provider}", nameof(provider))
            };
        }

        public static T? GetConcreteService<T>() where T : class
        {
            return typeof(T) switch
            {
                Type t when t == typeof(OpenAIService) => OpenAIService.Instance as T,
                Type t when t == typeof(AzureOpenAIService) => AzureOpenAIService.Instance as T,
                Type t when t == typeof(GoogleAIService) => GoogleAIService.Instance as T,
                Type t when t == typeof(AnthropicService) => AnthropicService.Instance as T,
                Type t when t == typeof(HuggingFaceService) => HuggingFaceService.Instance as T,
                Type t when t == typeof(OllamaService) => OllamaService.Instance as T,
                Type t when t == typeof(TongyiService) => TongyiService.Instance as T,
                _ => null
            };
        }
    }
}