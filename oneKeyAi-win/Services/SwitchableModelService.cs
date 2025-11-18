using System;
using System.Threading.Tasks;

namespace oneKeyAi_win.Services
{
    public class SwitchableModelService : ILargeModelService
    {
        private ModelProvider _currentProvider = ModelProvider.Tongyi;
     
        public SwitchableModelService()
        {
        }

        public void SwitchProvider(ModelProvider provider)
        {
            _currentProvider = provider;
        }

        public ModelProvider GetCurrentProvider()
        {
            return _currentProvider;
        }

        private ILargeModelService GetServiceForProvider(ModelProvider provider)
        {
            var service = LargeModelServiceFactory.GetService(provider);
            if (service is ILargeModelService typedService)
            {
                return typedService;
            }
            else
            {
                throw new InvalidOperationException($"Service for provider {provider} does not implement ILargeModelService");
            }
        }

        public async Task<ITextResponse> GenerateTextAsync(string model, string prompt, double temperature = 0.7, int maxTokens = 1000)
        {
            var currentService = GetServiceForProvider(_currentProvider);
            return await currentService.GenerateTextAsync(model, prompt, temperature, maxTokens);
        }

        public void SetApiKey(string apiKey)
        {
            var currentService = GetServiceForProvider(_currentProvider);
            currentService.SetApiKey(apiKey);
        }

        public void SetBaseUrl(string baseUrl)
        {
            var currentService = GetServiceForProvider(_currentProvider);
            currentService.SetBaseUrl(baseUrl);
        }
    }
}