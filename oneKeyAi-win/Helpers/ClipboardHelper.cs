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
                    SwitchableModelService? switchableService = null;

                    // Check which specific service is being used
                    if (modelService is SwitchableModelService svc)
                    {
                        switchableService = svc;
                        Debug.WriteLine($"当前使用的模型提供商: {switchableService.GetCurrentProvider()}");
                    }
                    else
                    {
                        Debug.WriteLine($"使用的服务类型: {modelService.GetType().Name}");
                    }

                    try
                    {
                        // 根据当前模型提供商选择相应的模型名称
                        string modelName = GetModelNameForProvider(switchableService?.GetCurrentProvider());
                        var result = await modelService.GenerateTextAsync(modelName, prompt);

                        // The result is now a standardized ITextResponse, so we can directly access the Content
                        string translatedText = result.Content;
                        Debug.WriteLine($"翻译结果：\n {translatedText}");

                        AppNotification notification = new AppNotificationBuilder()
                        .AddText($"{clipboardText}")
                        .AddText($"{translatedText}")
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


        /// <summary>
        /// 根据模型提供商返回相应的模型名称
        /// </summary>
        private static string GetModelNameForProvider(ModelProvider? provider)
        {
            return provider switch
            {
                ModelProvider.OpenAI => "gpt-3.5-turbo", // 或其他适当的OpenAI模型
                ModelProvider.AzureOpenAI => "gpt-35-turbo", // Azure OpenAI模型名称
                ModelProvider.GoogleAI => "gemini-2.5-flash", // Google AI模型名称
                ModelProvider.Anthropic => "claude-3-opus-20240229", // Anthropic模型名称
                ModelProvider.HuggingFace => "microsoft/DialoGPT-medium", // Hugging Face模型名称
                ModelProvider.Ollama => "llama2", // Ollama模型名称
                ModelProvider.Tongyi => "qwen-plus", // 通义千问模型名称
                _ => "qwen-plus" // 默认使用通义千问
            };
        }
    }
}
