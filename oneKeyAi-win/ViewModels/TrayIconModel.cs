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
    public partial class TrayIconModel : ObservableObject
    {
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
