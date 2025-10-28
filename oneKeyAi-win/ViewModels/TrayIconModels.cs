using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using oneKeyAi_win.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace oneKeyAi_win.ViewModels
{
    public partial class TrayIconModels : ObservableObject
    {
        [RelayCommand]
        private async void Test()
        {
            System.Diagnostics.Debug.WriteLine($"11111");
            UserConfig CC = new UserConfig()
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
            // Logic to show/hide a window on demand
            Console.WriteLine("Hello, World!");
        }

        [RelayCommand]
        private void ExitApplication()
        {
            Console.WriteLine("Hello, World!");
        }
    }
}
