using System;
using System.IO;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace MxbcRobOrderWinFormsApp
{
    public static class ConfigurationHelper
    {
        private static readonly string _filePath;
        private static IConfigurationRoot _configuration;

        static ConfigurationHelper()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "AppSettingsMxbc.json");
            LoadConfiguration();
        }

        private static void LoadConfiguration()
        {
            if (!File.Exists(_filePath))
            {
                CreateDefaultConfiguration();
            }

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(_filePath, optional: false, reloadOnChange: true);

            _configuration = builder.Build();
        }

        private static void CreateDefaultConfiguration()
        {
            var defaultSettings = new
            {
                AppSettings = new
                {
                    textBoxDelayRobOrder = "1500",
                    textBoxOrderCount = "15",
                    textBoxSerivceRefresh = "5000",
                    checkBoxIsProxy = false,
                    textBoxProxyInfo = "127.0.0.1:8080",
                    textBoxSecretWord = "茉莉奶绿 白月光",
                }
            };

            var json = JsonConvert.SerializeObject(defaultSettings, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }

        public static string GetAppSetting(string key)
        {
            return _configuration[$"AppSettings:{key}"];
        }

        public static int GetAppSettingAsInt(string key)
        {
            if (int.TryParse(_configuration[$"AppSettings:{key}"], out int result))
            {
                return result;
            }
            throw new InvalidCastException($"The key '{key}' does not contain a valid integer.");
        }

        public static bool GetAppSettingAsBool(string key)
        {
            if (bool.TryParse(_configuration[$"AppSettings:{key}"], out bool result))
            {
                return result;
            }
            throw new InvalidCastException($"The key '{key}' does not contain a valid boolean.");
        }

        public static void SetAppSetting(string key, string value)
        {
            var json = File.ReadAllText(_filePath);
            dynamic jsonObj = JsonConvert.DeserializeObject(json);

            jsonObj.AppSettings[key] = value;

            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(_filePath, output);

            LoadConfiguration(); // 重新加载配置以反映更改
        }

        public static void SetAppSetting(string key, int value)
        {
            SetAppSetting(key, value.ToString());
        }

        public static void SetAppSetting(string key, bool value)
        {
            SetAppSetting(key, value.ToString().ToLower());
        }

        public static void PrintSettings()
        {
            foreach (var section in _configuration.GetSection("AppSettings").GetChildren())
            {
                Console.WriteLine($"{section.Key}: {section.Value}");
            }
        }
    }
}
