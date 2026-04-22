using System.Data;
using System.Text.RegularExpressions;

namespace DataArenaFusion.Core.Services.Database
{
    public static class DatabaseMigrationHelper
    {
        public static string NormalizeIdentifier(string value, string fallback)
        {
            var cleaned = Regex.Replace(value ?? string.Empty, @"[^\p{L}\p{Nd}_]+", "_");
            cleaned = Regex.Replace(cleaned, @"_+", "_").Trim('_');

            if (string.IsNullOrWhiteSpace(cleaned))
            {
                cleaned = fallback;
            }

            if (char.IsDigit(cleaned[0]))
            {
                cleaned = "_" + cleaned;
            }

            return cleaned;
        }

        public static string EnsureUniqueName(IEnumerable<string> existingNames, string baseName)
        {
            var names = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);
            if (!names.Contains(baseName))
            {
                return baseName;
            }

            var index = 2;
            string candidate;
            do
            {
                candidate = $"{baseName}_{index}";
                index++;
            } while (names.Contains(candidate));

            return candidate;
        }

        public static IEnumerable<IGrouping<string, DataRow>> GroupBySource(DataTable table, string sourceColumn)
        {
            return table.AsEnumerable()
                .Where(row => row.Table.Columns.Contains(sourceColumn))
                .GroupBy(row => row[sourceColumn]?.ToString() ?? "datos");
        }
    }
}
