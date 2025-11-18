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
        private readonly SwitchableModelService? _switchableModelService;

        public TrayIconModel()
        {
            // Get the switchable model service from the app's service provider
            _switchableModelService = (App.ServiceProvider?.GetService(typeof(ILargeModelService)) as SwitchableModelService);

            // Initialize the current model provider description
            OnPropertyChanged(nameof(CurrentModelProviderDescription));
        }

        public string CurrentModelProviderDescription
        {
            get
            {
                if (_switchableModelService != null)
                {
                    var currentProvider = _switchableModelService.GetCurrentProvider();
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
            if (_switchableModelService != null)
            {
                _switchableModelService.SwitchProvider(provider);
                OnPropertyChanged(nameof(CurrentModelProviderDescription));
            }
        }

        [RelayCommand]
        private void RefreshProviderStatus()
        {
            OnPropertyChanged(nameof(CurrentModelProviderDescription));
        }

        [RelayCommand]
        private async Task Test()
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
        private void ShowHideWindow()
        {
            // Logic to show the MainWindow
            var app = (App)App.Current;
            // Use the app's method to create or get the main window
            app.ToggleMainWindow();
        }

        [RelayCommand]
        private void ExitApplication()
        {
            var app = (App)App.Current;
            app._window?.Close();
            System.Environment.Exit(0); // Force exit the application since tray apps don't close with main window
        }
    }
}
