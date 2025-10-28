using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace oneKeyAi_win.Configuration
{
    internal class ConfigService
    {
        // C:\Users\<用户名>\AppData\Roaming\MyApp\config.json
        private static readonly string AppFolder = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "oneKey");
        private static readonly string ConfigPath = Path.Combine(AppFolder, "config.json");
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true // 美化格式
        };
        /// <summary>
        /// 加载配置文件（如果不存在则创建默认配置）
        /// </summary>
        public static async Task<UserConfig> LoadAsync()
        {
            try
            {
                if (!Directory.Exists(AppFolder))
                    Directory.CreateDirectory(AppFolder);

                if (!File.Exists(ConfigPath))
                {
                    var defaultConfig = new UserConfig();
                    await SaveAsync(defaultConfig);
                    return defaultConfig;
                }

                string json = await File.ReadAllTextAsync(ConfigPath);
                return JsonSerializer.Deserialize<UserConfig>(json) ?? new UserConfig();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"加载配置出错: {ex.Message}");
                return new UserConfig();
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public static async Task SaveAsync(UserConfig config)
        {
            try
            {
                if (!Directory.Exists(AppFolder))
                    Directory.CreateDirectory(AppFolder);

                string json = JsonSerializer.Serialize(config, JsonOptions);
                System.Diagnostics.Debug.WriteLine($"保存配置文件位置: {ConfigPath}");
                await File.WriteAllTextAsync(ConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存配置出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取当前配置文件路径（方便调试或显示给用户）
        /// </summary>
        public static string GetConfigPath() => ConfigPath;

    }

}
