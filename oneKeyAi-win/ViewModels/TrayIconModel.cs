using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using oneKeyAi_win.Configuration;
using oneKeyAi_win.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace oneKeyAi_win.ViewModels
{
    public partial class TrayIconModel : ObservableObject
    {
        private readonly SwitchableModelService? modelService;

        public TrayIconModel()
        {
            // Get the switchable model service from the app's service provider
            modelService = (App.ServiceProvider?.GetService(typeof(ILargeModelService)) as SwitchableModelService);

            // Set the default API key for the default provider (Tongyi)
            modelService?.SetApiKey("sk-3ab003e0b90346e58d4072f402a15b13");

            // Initialize the current model provider description
            OnPropertyChanged(nameof(CurrentModelProviderDescription));
        }

        public string CurrentModelProviderDescription
        {
            get
            {
                if (modelService != null)
                {
                    var currentProvider = modelService.GetCurrentProvider();
                    return currentProvider.GetDescription();
                }
              
                return ModelProvider.Tongyi.GetDescription(); // Default to Tongyi
            }
        }

        [RelayCommand]
        private void SwitchToOpenAI()
        {
            SwitchProvider(ModelProvider.OpenAI);
        }

        [RelayCommand]
        private void SwitchToAzureOpenAI()
        {
            SwitchProvider(ModelProvider.AzureOpenAI);
        }

        [RelayCommand]
        private void SwitchToGoogleAI()
        {
            SwitchProvider(ModelProvider.GoogleAI);
        }

        [RelayCommand]
        private void SwitchToAnthropic()
        {
            SwitchProvider(ModelProvider.Anthropic);
        }

        [RelayCommand]
        private void SwitchToHuggingFace()
        {
            SwitchProvider(ModelProvider.HuggingFace);
        }

        [RelayCommand]
        private void SwitchToOllama()
        {
            SwitchProvider(ModelProvider.Ollama);
        }

        [RelayCommand]
        private void SwitchToTongyi()
        {
            SwitchProvider(ModelProvider.Tongyi);
        }

        private void SwitchProvider(ModelProvider provider)
        {
            if (modelService != null)
            {
                modelService.SwitchProvider(provider);

                // 根据provider类型设置对应的API密钥
                switch (provider)
                {
                    case ModelProvider.OpenAI:
                    case ModelProvider.AzureOpenAI:
                    case ModelProvider.Ollama:
                    case ModelProvider.Tongyi:
                        // 为OpenAI、Azure OpenAI、Ollama、通义千问设置API密钥
                        modelService.SetApiKey("sk-3ab003e0b90346e58d4072f402a15b13");
                        break;
                    case ModelProvider.GoogleAI:
                        // 为Google AI设置API密钥
                        modelService.SetApiKey("AIzaSyBSxB5hopBfj4Blms6Kf7l_lHyjN_EfAnc");
                        break;
                    case ModelProvider.Anthropic:
                    case ModelProvider.HuggingFace:
                        // 为Anthropic和Hugging Face设置API密钥
                        // 可以替换为真实的API密钥
                        modelService.SetApiKey("hf_YourHuggingFaceApiKey");
                        break;
                    default:
                        // 默认API密钥
                        modelService.SetApiKey("sk-3ab003e0b90346e58d4072f402a15b13");
                        break;
                }

                OnPropertyChanged(nameof(CurrentModelProviderDescription));
            }
        }

        [RelayCommand]
        private void RefreshProviderStatus()
        {
            OnPropertyChanged(nameof(CurrentModelProviderDescription));
        }

        [RelayCommand]
        private static async Task Test()
        {
            System.Diagnostics.Debug.WriteLine($"11111");
            UserConfig CC = new ()
            {
                Theme = "小王子",
                Language = "en",
                ApiEndpoint = "https://my.test.com",
                RefreshInterval = 99,
            };
            await ConfigService.SaveAsync(CC);
            UserConfig Config = await ConfigService.LoadAsync();
            System.Diagnostics.Debug.WriteLine($"配置文件路径: {JsonSerializer.Serialize(Config)}");
            System.Diagnostics.Debug.WriteLine($"配置文件路径: {ConfigService.GetConfigPath()}");
        }
        [RelayCommand]
        private static void ShowHideWindow()
        {
            // Logic to show the MainWindow
            var app = (App)App.Current;
            // Use the app's method to create or get the main window
            app.ToggleMainWindow();
        }

        [RelayCommand]
        private static void ExitApplication()
        {
            var app = (App)App.Current;
            app._window?.Close();
            System.Environment.Exit(0); // Force exit the application since tray apps don't close with main window
        }
    }
}
