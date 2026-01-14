using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace M59AdminTool.Services
{
    public static class TabOrderService
    {
        private static string GetSettingsPath()
        {
            var folder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(folder, "M59AdminTool", "tab-order.json");
        }

        public static void ApplyOrder(System.Windows.Controls.TabControl tabControl)
        {
            var order = LoadOrder();
            if (order.Count == 0)
                return;

            var items = tabControl.Items.OfType<System.Windows.Controls.TabItem>().ToList();
            if (items.Count == 0)
                return;

            var byKey = items
                .Select(item => (item, key: GetKey(item)))
                .Where(entry => !string.IsNullOrWhiteSpace(entry.key))
                .ToDictionary(entry => entry.key, entry => entry.item, StringComparer.OrdinalIgnoreCase);

            var ordered = new List<System.Windows.Controls.TabItem>();
            foreach (var key in order)
            {
                if (byKey.TryGetValue(key, out var item) && !ordered.Contains(item))
                {
                    ordered.Add(item);
                }
            }

            foreach (var item in items)
            {
                if (!ordered.Contains(item))
                    ordered.Add(item);
            }

            tabControl.Items.Clear();
            foreach (var item in ordered)
            {
                tabControl.Items.Add(item);
            }
        }

        public static void SaveOrder(System.Windows.Controls.TabControl tabControl)
        {
            var keys = tabControl.Items
                .OfType<System.Windows.Controls.TabItem>()
                .Select(GetKey)
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .ToList();

            if (keys.Count == 0)
                return;

            var path = GetSettingsPath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonSerializer.Serialize(keys, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        private static List<string> LoadOrder()
        {
            var path = GetSettingsPath();
            if (!File.Exists(path))
                return new List<string>();

            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        private static string GetKey(System.Windows.Controls.TabItem item)
        {
            if (item.Tag is string tag && !string.IsNullOrWhiteSpace(tag))
                return tag;

            if (item is System.Windows.FrameworkElement element && !string.IsNullOrWhiteSpace(element.Name))
                return element.Name;

            return item.Header?.ToString() ?? string.Empty;
        }
    }
}
