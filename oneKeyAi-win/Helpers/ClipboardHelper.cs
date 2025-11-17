using Microsoft.Extensions.DependencyInjection;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using oneKeyAi_win.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using WindowsInput.Events;

namespace oneKeyAi_win.Helpers
{
    internal class ClipboardHelper
    {

        public static async void OneKey()
        {
            await WindowsInput.Simulate.Events().Wait(1000).ClickChord(KeyCode.Control, KeyCode.C).Invoke();
            await Task.Delay(200); // 等待 200 毫秒
            string? clipboardText = await GetClipboardTextAsync();

            string prompt = $"{clipboardText}\n以上内容如果是中文则翻译成英文，如果是英文则翻译成中文";
            Debug.WriteLine($"prompt: {prompt}");
            if (!string.IsNullOrWhiteSpace(clipboardText))
            {
                var modelService = App.ServiceProvider?.GetRequiredService<ILargeModelService>();
                if (modelService != null)
                {
                    modelService.SetApiKey("sk-3ab003e0b90346e58d4072f402a15b13");
                    // Check which specific service is being used
                    if (modelService is SwitchableModelService switchableService)
                    {
                        Debug.WriteLine($"当前使用的模型提供商: {switchableService.GetCurrentProvider()}");
                    }
                    else
                    {
                        Debug.WriteLine($"使用的服务类型: {modelService.GetType().Name}");
                    }

                    try
                    {
                        var result = await modelService.GenerateTextAsync("qwen-plus", "Hello!");
                        Debug.WriteLine($"翻译结果：\n {result.ToString()}");

                        AppNotification notification = new AppNotificationBuilder()
                        .AddText($"{clipboardText}")
                        //.AddText($"{result?.Response}")
                        .BuildNotification();

                        AppNotificationManager.Default.Show(notification);

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"调用服务失败: {ex.Message}");
                    }
                }
                else
                {
                    Debug.WriteLine("服务未初始化");
                }
            }
            else
            {
                Debug.WriteLine("剪切板为空或不是文本");
            }
        }
        /// <summary>
        /// 异步读取剪切板中的文本（如果有）
        /// </summary>
        private static async Task<string?> GetClipboardTextAsync()
        {
            try
            {
                var clipboard = Clipboard.GetContent();
                if (clipboard.Contains(StandardDataFormats.Text))
                {
                    return await clipboard.GetTextAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"读取剪切板失败: {ex.Message}");
            }

            return null;
        }
    }
}
