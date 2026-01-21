using System;
using System.IO;
using System.Text.Json;

namespace M59AdminTool.Services
{
    public sealed class AppSettings
    {
        public string ServerRootPath { get; set; } = string.Empty;
        public string KodPath { get; set; } = string.Empty;
    }

    public static class SettingsService
    {
        private static string GetSettingsPath()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(folder, "M59AdminTool", "settings.json");
        }

        public static AppSettings Load()
        {
            var path = GetSettingsPath();
            if (!File.Exists(path))
                return new AppSettings();

            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public static void Save(AppSettings settings)
        {
            var path = GetSettingsPath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
    }
}
