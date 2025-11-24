using H.NotifyIcon;
using H.NotifyIcon.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using oneKeyAi_win.Configuration;
using oneKeyAi_win.Helpers;
using oneKeyAi_win.Services;
using oneKeyAi_win.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace oneKeyAi_win
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        [LibraryImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetForegroundWindow(IntPtr hWnd);

        [LibraryImport("user32.dll", EntryPoint = "ShowWindow")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;
        public MainWindow? _window;
        public static IServiceProvider? ServiceProvider { get; private set; }
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            ConfigureServices();
        }

        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register the switchable model service as a singleton
            services.AddSingleton<ILargeModelService, SwitchableModelService>();

            // Register other services as singletons if needed
            services.AddSingleton<OpenAIService>(provider => OpenAIService.Instance);
            services.AddSingleton<AzureOpenAIService>(provider => AzureOpenAIService.Instance);
            services.AddSingleton<GoogleAIService>(provider => GoogleAIService.Instance);
            services.AddSingleton<AnthropicService>(provider => AnthropicService.Instance);
            services.AddSingleton<HuggingFaceService>(provider => HuggingFaceService.Instance);
            services.AddSingleton<OllamaService>(provider => OllamaService.Instance);
            services.AddSingleton<TongyiService>(provider => TongyiService.Instance);

            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            InitializeTrayIcon();
            HotkeyHelper.RegisterGlobalHotkeys();
        }

        private void InitializeTrayIcon()
        {
            if (Resources["TrayIcon"] is TaskbarIcon trayIcon)
            {
                trayIcon.ForceCreate();
            }
        }

        public void ToggleMainWindow()
        {
            if (_window == null)
            {
                // 1. 窗口不存在: 创建、设置事件、显示并置顶
                _window = new MainWindow();
                _window.Closed += (sender, args) => _window = null;
                _window.Activate();
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
                ShowWindow(hwnd, SW_RESTORE);
                SetForegroundWindow(hwnd);
            }
            else
            {
                // 2. 窗口已存在: 检查状态并置顶，或什么也不做
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);
                // 窗口已存在但已隐藏或不在最上层，显示并置顶
                _window.Show();
                _window.Activate(); // 激活 WinUI 窗口
                ShowWindow(hwnd, SW_RESTORE); // 确保从最小化状态恢复
                SetForegroundWindow(hwnd); // 确保窗口位于所有其他窗口之上
            }
        }
    }
}
