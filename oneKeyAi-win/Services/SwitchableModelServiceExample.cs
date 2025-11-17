using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace oneKeyAi_win.Services
{
    /// <summary>
    /// Example showing how to use the SwitchableModelService with dependency injection
    /// for runtime switching between different AI services
    /// </summary>
    public class SwitchableModelServiceExample
    {
        public static async Task ExampleUsage()
        {
            // Get the service from DI container (this would typically be injected in a ViewModel or Service)
            var modelService = App.ServiceProvider?.GetRequiredService<ILargeModelService>();
            
            if (modelService is SwitchableModelService switchableService)
            {
                // Initially using OpenAI (the default)
                Console.WriteLine($"Current provider: {switchableService.GetCurrentProvider()}");
                
                // Set API key for OpenAI
                switchableService.SetApiKey("your-openai-api-key");
                
                try
                {
                    var openAiResult = await modelService.GenerateTextAsync("gpt-3.5-turbo", "Hello, world!");
                    Console.WriteLine("OpenAI Response received");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error with OpenAI: {ex.Message}");
                }
                
                // Switch to Ollama
                switchableService.SwitchProvider(ModelProvider.Ollama);
                Console.WriteLine($"Switched to provider: {switchableService.GetCurrentProvider()}");
                
                // No API key needed for Ollama (typically runs locally)
                try
                {
                    var ollamaResult = await modelService.GenerateTextAsync("llama2", "Hello, from Ollama!");
                    Console.WriteLine("Ollama Response received");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error with Ollama: {ex.Message}");
                }
                
                // Switch to Google AI
                switchableService.SwitchProvider(ModelProvider.GoogleAI);
                Console.WriteLine($"Switched to provider: {switchableService.GetCurrentProvider()}");
                
                switchableService.SetApiKey("your-google-api-key");
                try
                {
                    var googleResult = await modelService.GenerateTextAsync("gemini-pro", "Hello, from Google!");
                    Console.WriteLine("Google AI Response received");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error with Google AI: {ex.Message}");
                }
                
                // Switch to Azure OpenAI
                switchableService.SwitchProvider(ModelProvider.AzureOpenAI);
                Console.WriteLine($"Switched to provider: {switchableService.GetCurrentProvider()}");
                
                switchableService.SetApiKey("your-azure-api-key");
                switchableService.SetBaseUrl("https://your-resource-name.openai.azure.com");
                // For Azure, you might also need to set the deployment name
                if (LargeModelServiceFactory.GetConcreteService<AzureOpenAIService>() is AzureOpenAIService azureService)
                {
                    azureService.SetDeploymentName("your-deployment-name");
                }
                
                try
                {
                    var azureResult = await modelService.GenerateTextAsync("gpt-35-turbo", "Hello, from Azure!");
                    Console.WriteLine("Azure OpenAI Response received");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error with Azure OpenAI: {ex.Message}");
                }
            }
        }
    }
}