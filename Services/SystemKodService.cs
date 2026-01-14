using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace M59AdminTool.Services
{
    public class SystemKodService
    {
        private static readonly Regex ClassRegex = new Regex(
            @"&(?<name>[A-Za-z_][A-Za-z0-9_]*)",
            RegexOptions.Compiled
        );

        public IReadOnlyList<string> LoadClassNames()
        {
            var path = FindSystemKodPath();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return Array.Empty<string>();
            }

            var text = File.ReadAllText(path);
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (Match match in ClassRegex.Matches(text))
            {
                var name = match.Groups["name"].Value;
                if (!string.IsNullOrWhiteSpace(name))
                {
                    set.Add(name);
                }
            }

            return set.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static string? FindSystemKodPath()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var current = new DirectoryInfo(baseDir);
            for (var depth = 0; depth < 6 && current != null; depth++)
            {
                var direct = Path.Combine(current.FullName, "kod", "util", "system.kod");
                if (File.Exists(direct))
                {
                    return direct;
                }

                foreach (var serverDir in current.EnumerateDirectories("Server-104-main*"))
                {
                    var candidate = Path.Combine(serverDir.FullName, "kod", "util", "system.kod");
                    if (File.Exists(candidate))
                    {
                        return candidate;
                    }
                }

                current = current.Parent;
            }

            return null;
        }
    }
}
