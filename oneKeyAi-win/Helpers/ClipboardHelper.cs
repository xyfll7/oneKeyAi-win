using Microsoft.Extensions.DependencyInjection;
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
            Debug.WriteLine($"剪切板内容11：{clipboardText}");
            if (!string.IsNullOrWhiteSpace(clipboardText))
            {
                var ollama = App.ServiceProvider?.GetRequiredService<OllamaService>();
                if (ollama != null)
                {
                    try
                    {
                        var result = await ollama.GenerateAsync("deepseek-r1:8b", "你好");
                        Debug.WriteLine($"剪切板内容：{clipboardText} {result?.Response ?? "No response"}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"调用Ollama服务失败: {ex.Message}");
                    }
                }
                else
                {
                    Debug.WriteLine("Ollama服务未初始化");
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
