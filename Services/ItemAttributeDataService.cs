using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace M59AdminTool.Services
{
    public class ItemAttributeDataService
    {
        private readonly string? _blakstonPath;

        public ItemAttributeDataService()
        {
            _blakstonPath = ResolveBlakstonPath();
        }

        public async Task<Dictionary<int, string>> LoadItemAttributesAsync()
        {
            if (string.IsNullOrWhiteSpace(_blakstonPath) || !File.Exists(_blakstonPath))
                return new Dictionary<int, string>();

            var lines = await ReadAllLinesAsync(_blakstonPath);
            var map = new Dictionary<int, string>();
            var regex = new Regex(@"^\s*((?:IA|WA)_[A-Za-z0-9_]+)\s*=\s*(\d+)", RegexOptions.Compiled);

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (!match.Success)
                    continue;

                var name = match.Groups[1].Value.Trim();
                if (!int.TryParse(match.Groups[2].Value, out var id))
                    continue;

                if (!map.ContainsKey(id))
                {
                    map[id] = name;
                }
            }

            return map;
        }

        public async Task<Dictionary<int, string>> LoadAttackSpellTypesAsync()
        {
            if (string.IsNullOrWhiteSpace(_blakstonPath) || !File.Exists(_blakstonPath))
                return new Dictionary<int, string>();

            var lines = await ReadAllLinesAsync(_blakstonPath);
            var map = new Dictionary<int, string>();
            var regex = new Regex(@"^\s*(ATCK_SPELL_[A-Za-z0-9_]+)\s*=\s*(0x[0-9A-Fa-f]+|\d+)",
                RegexOptions.Compiled);

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (!match.Success)
                    continue;

                var name = match.Groups[1].Value.Trim();
                var valueText = match.Groups[2].Value.Trim();
                if (!TryParseInt(valueText, out var value))
                    continue;

                if (!map.ContainsKey(value))
                {
                    map[value] = name;
                }
            }

            return map;
        }

        private static Encoding GetKodEncoding()
        {
            return Encoding.GetEncoding(1252);
        }

        private static async Task<string[]> ReadAllLinesAsync(string path)
        {
            return await File.ReadAllLinesAsync(path, GetKodEncoding());
        }

        private static bool TryParseInt(string value, out int result)
        {
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                return int.TryParse(value.Substring(2), System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture, out result);
            }

            return int.TryParse(value, out result);
        }

        private static string? ResolveBlakstonPath()
        {
            var candidates = new[]
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Environment.CurrentDirectory
            };

            foreach (var start in candidates)
            {
                var dir = new DirectoryInfo(start);
                for (var i = 0; i < 6 && dir != null; i++)
                {
                    var candidate = Path.Combine(dir.FullName, "Server-104-main", "kod", "include", "blakston.khd");
                    if (File.Exists(candidate))
                        return candidate;

                    dir = dir.Parent;
                }
            }

            return null;
        }
    }
}
