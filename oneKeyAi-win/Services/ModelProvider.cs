using System.ComponentModel;
using System.Linq;

namespace oneKeyAi_win.Services
{
    public enum ModelProvider
    {
        [Description("OpenAI")]
        OpenAI,
        [Description("Azure OpenAI")]
        AzureOpenAI,
        [Description("Google AI")]
        GoogleAI,
        [Description("Anthropic")]
        Anthropic,
        [Description("Hugging Face")]
        HuggingFace,
        [Description("Ollama")]
        Ollama,
        [Description("通义千问")]
        Tongyi
    }
    public static class ModelProviderExtensions
    {
        public static string GetDescription(this ModelProvider provider)
        {
            var field = provider.GetType().GetField(provider.ToString());
            var attribute = field?.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault() as DescriptionAttribute;
            return attribute?.Description ?? provider.ToString();
        }
    }
}