# Large Model Services

This directory contains HTTP request classes for various large language model providers. Each service class handles the API communication with its respective provider.

## Available Services

- **OpenAIService**: For OpenAI's ChatGPT, GPT-3.5, GPT-4 models
- **AzureOpenAIService**: For Azure OpenAI Service
- **GoogleAIService**: For Google's Gemini (formerly PaLM) models
- **AnthropicService**: For Anthropic's Claude models
- **HuggingFaceService**: For Hugging Face Inference API
- **OllamaService**: For local Ollama models
- **TongyiService**: For Tongyi/Qwen (通义千问) models from Alibaba Cloud
- **LargeModelServiceFactory**: Factory to get service instances

## Usage Examples

### Using the Factory

```csharp
// Get a specific service using the factory (returns object, cast as needed)
var openAIService = LargeModelServiceFactory.GetService(ModelProvider.OpenAI) as OpenAIService;
var googleAIService = LargeModelServiceFactory.GetService(ModelProvider.GoogleAI) as GoogleAIService;
var tongyiService = LargeModelServiceFactory.GetService(ModelProvider.Tongyi) as TongyiService;

// Or get the concrete service directly
var ollamaService = LargeModelServiceFactory.GetConcreteService<OllamaService>();
var tongyiService = LargeModelServiceFactory.GetConcreteService<TongyiService>();
```

### Direct Service Usage

Each service follows a similar pattern:

```csharp
// Initialize service with API key and optionally a custom base URL
var service = new OpenAIService("your-api-key", "https://api.openai.com/v1");

// Or use the singleton instance and set the API key
OpenAIService.Instance.SetApiKey("your-api-key");
OpenAIService.Instance.SetBaseUrl("https://api.openai.com/v1");

// Make requests
var result = await service.ChatCompletionsAsync(
    model: "gpt-3.5-turbo",
    prompt: "Hello, how are you?",
    temperature: 0.7
);
```

### Supported Providers

The system supports the following model providers:

- OpenAI: GPT-3.5, GPT-4, and other models
- Azure OpenAI: Azure-hosted OpenAI models
- Google AI: Gemini (formerly PaLM) models
- Anthropic: Claude models
- Hugging Face: Models hosted on Hugging Face Hub
- Ollama: Local open-source models
- Tongyi/Qwen: Alibaba Cloud's Qwen (通义千问) models

Each service class implements proper error handling and follows the same structure patterns for consistency.