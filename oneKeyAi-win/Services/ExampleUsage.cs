using System;
using System.Threading.Tasks;

namespace oneKeyAi_win.Services
{
    /// <summary>
    /// Example usage of the different large language model services
    /// This is for demonstration purposes to show how to use the services
    /// </summary>
    public class ExampleUsage
    {
        public static async Task ExampleOpenAIUsage()
        {
            // Configure OpenAI service
            var openAIService = OpenAIService.Instance;
            openAIService.SetApiKey("your-openai-api-key-here");
            openAIService.SetBaseUrl("https://api.openai.com/v1"); // Optional, defaults to this
            
            try
            {
                var response = await openAIService.ChatCompletionsAsync(
                    model: "gpt-3.5-turbo",
                    prompt: "Hello, how are you?",
                    temperature: 0.7,
                    maxTokens: 150
                );
                
                if (response?.Choices?.Count > 0)
                {
                    var result = response.Choices[0].Message?.Content;
                    Console.WriteLine($"OpenAI Response: {result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with OpenAI service: {ex.Message}");
            }
        }

        public static async Task ExampleAzureOpenAIUsage()
        {
            // Configure Azure OpenAI service
            var azureService = AzureOpenAIService.Instance;
            azureService.SetApiKey("your-azure-api-key-here");
            azureService.SetBaseUrl("https://your-resource-name.openai.azure.com");
            azureService.SetDeploymentName("your-deployment-name");
            
            try
            {
                var messages = new System.Collections.Generic.List<Message>
                {
                    new Message { Role = "user", Content = "Hello, how are you?" }
                };
                
                var response = await azureService.ChatCompletionsAsync(
                    messages: messages,
                    temperature: 0.7,
                    maxTokens: 150
                );
                
                if (response?.Choices?.Count > 0)
                {
                    var result = response.Choices[0].Message?.Content;
                    Console.WriteLine($"Azure OpenAI Response: {result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with Azure OpenAI service: {ex.Message}");
            }
        }

        public static async Task ExampleGoogleAIUsage()
        {
            // Configure Google AI service (Gemini)
            var googleService = GoogleAIService.Instance;
            googleService.SetApiKey("your-google-api-key-here");
            googleService.SetBaseUrl("https://generativelanguage.googleapis.com/v1beta"); // Optional
            
            try
            {
                var response = await googleService.GenerateContentAsync(
                    model: "gemini-pro",
                    prompt: "Hello, how are you?",
                    temperature: 0.7,
                    maxOutputTokens: 150
                );
                
                if (response?.Candidates?.Count > 0)
                {
                    var result = response.Candidates[0].Content?.Parts?[0].Text;
                    Console.WriteLine($"Google AI Response: {result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with Google AI service: {ex.Message}");
            }
        }

        public static async Task ExampleAnthropicUsage()
        {
            // Configure Anthropic service (Claude)
            var anthropicService = AnthropicService.Instance;
            anthropicService.SetApiKey("your-anthropic-api-key-here");
            anthropicService.SetBaseUrl("https://api.anthropic.com/v1"); // Optional
            
            try
            {
                var response = await anthropicService.MessagesAsync(
                    model: "claude-3-haiku-20240307",
                    prompt: "Hello, how are you?",
                    temperature: 0.7,
                    maxTokens: 150
                );
                
                if (response?.Content?.Count > 0)
                {
                    var result = response.Content[0].Text;
                    Console.WriteLine($"Anthropic Response: {result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with Anthropic service: {ex.Message}");
            }
        }

        public static async Task ExampleHuggingFaceUsage()
        {
            // Configure Hugging Face service
            var huggingFaceService = HuggingFaceService.Instance;
            huggingFaceService.SetApiKey("your-huggingface-api-key-here");
            
            try
            {
                var responses = await huggingFaceService.TextGenerationAsync(
                    model: "gpt2",
                    prompt: "Hello, how are you?",
                    temperature: 0.7,
                    maxNewTokens: 100
                );
                
                if (responses?.Count > 0)
                {
                    var result = responses[0].GeneratedText;
                    Console.WriteLine($"Hugging Face Response: {result}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with Hugging Face service: {ex.Message}");
            }
        }

        public static async Task ExampleOllamaUsage()
        {
            // Configure Ollama service
            var ollamaService = OllamaService.Instance;
            // Ollama doesn't need an API key by default
            // The base URL can be set during construction or remains as default

            try
            {
                var response = await ollamaService.GenerateAsync(
                    model: "llama2",
                    think: false,
                    prompt: "Hello, how are you?"
                );

                if (!string.IsNullOrEmpty(response?.Response))
                {
                    Console.WriteLine($"Ollama Response: {response.Response}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with Ollama service: {ex.Message}");
            }
        }

        public static async Task ExampleTongyiUsage()
        {
            // Configure Tongyi service (Qwen/通义千问)
            var tongyiService = TongyiService.Instance;
            tongyiService.SetApiKey("your-tongyi-api-key-here");
            tongyiService.SetBaseUrl("https://dashscope.aliyuncs.com/api/v1"); // Optional, defaults to this

            try
            {
                var response = await tongyiService.ChatCompletionsAsync(
                    model: "qwen-max",
                    prompt: "Hello, how are you?",
                    temperature: 0.7,
                    maxTokens: 150
                );

                if (response?.Output?.Choices?.Count > 0)
                {
                    var result = response.Output.Choices[0].Message?.Content;
                    Console.WriteLine($"Tongyi Response: {result}");
                }
                else if (!string.IsNullOrEmpty(response?.Output?.Text))
                {
                    Console.WriteLine($"Tongyi Response: {response.Output.Text}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error with Tongyi service: {ex.Message}");
            }
        }

        public static void ExampleFactoryUsage()
        {
            // Using the factory to get services
            var openAIService = LargeModelServiceFactory.GetService(ModelProvider.OpenAI) as OpenAIService;
            if (openAIService != null)
            {
                openAIService.SetApiKey("your-api-key");
                Console.WriteLine("Retrieved OpenAI service via factory");
            }

            // Also try Tongyi service via factory
            var tongyiService = LargeModelServiceFactory.GetService(ModelProvider.Tongyi) as TongyiService;
            if (tongyiService != null)
            {
                tongyiService.SetApiKey("your-tongyi-api-key");
                Console.WriteLine("Retrieved Tongyi service via factory");
            }

            // Or get the concrete service directly
            var azureService = LargeModelServiceFactory.GetConcreteService<AzureOpenAIService>();
            if (azureService != null)
            {
                Console.WriteLine("Retrieved Azure service via factory GetConcreteService method");
            }

            var tongyiConcreteService = LargeModelServiceFactory.GetConcreteService<TongyiService>();
            if (tongyiConcreteService != null)
            {
                Console.WriteLine("Retrieved Tongyi service via factory GetConcreteService method");
            }
        }
    }
}