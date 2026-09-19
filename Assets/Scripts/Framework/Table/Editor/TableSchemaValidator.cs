#if UNITY_EDITOR
using System;
using System.Collections.Generic;

namespace Framework.Table.Editor
{
    internal static class TableSchemaValidator
    {
        private static readonly HashSet<string> SupportedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "byte", "int", "int32", "float", "single", "bool", "boolean", "string"
        };

        public static bool TryValidate(List<List<string>> grid, string fileName, out string error)
        {
            if (grid == null || grid.Count < 3)
            {
                error = $"'{fileName}' needs at least three rows: field names, field types, and one data row.";
                return false;
            }

            List<string> names = grid[0];
            List<string> types = grid[1];
            if (names == null || types == null || names.Count == 0 || names.Count != types.Count)
            {
                error = $"'{fileName}' has mismatched field-name and field-type column counts.";
                return false;
            }

            var seenHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int column = 0; column < names.Count; column++)
            {
                string name = names[column]?.Trim();
                string type = types[column]?.Trim();
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(type))
                {
                    error = $"'{fileName}' has a blank field name or type at column {column + 1}.";
                    return false;
                }
                if (!seenHeaders.Add(name))
                {
                    error = $"'{fileName}' contains duplicate field '{name}'.";
                    return false;
                }
                if (!SupportedTypes.Contains(type))
                {
                    error = $"'{fileName}' field '{name}' uses unsupported type '{type}'.";
                    return false;
                }
            }

            string firstField = names[0]?.Trim();
            if (string.Equals(firstField, "Key", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(firstField, "Id", StringComparison.OrdinalIgnoreCase))
            {
                var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                for (int row = 2; row < grid.Count; row++)
                {
                    if (grid[row] == null || grid[row].Count == 0) continue;
                    string key = grid[row][0]?.Trim();
                    if (string.IsNullOrEmpty(key)) continue;
                    if (!seenKeys.Add(key))
                    {
                        error = $"'{fileName}' contains duplicate key '{key}' at Excel row {row + 1}.";
                        return false;
                    }
                }
            }

            error = null;
            return true;
        }
    }
}
#endif
