using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Framework.Table
{
    /// <summary>
    /// Excel/CSV 2차원 셀 Grid 데이터를 JSON 포맷 문자열로 변환하는 데이터 파이프라인 변환기입니다.
    /// Header Row 0: 필드명 (Field Name)
    /// Header Row 1: 자료형 (Data Type: byte, int, float, bool, string)
    /// Row 2부터: 실제 데이터 행
    /// </summary>
    public static class TableFileConverter
    {
        /// <summary>
        /// 2차원 셀 Grid 데이터를 직렬화하여 표준 JSON 문자열을 생성합니다.
        /// </summary>
        public static string ConvertGridToJson(List<List<string>> grid, string fileName = "")
        {
            if (grid == null || grid.Count < 3)
            {
                Debug.LogError($"[TableFileConverter] Grid data must have at least 3 rows (Header Names, Data Types, Data Rows). File: '{fileName}'");
                return "{\n  \"data\": []\n}";
            }

            List<string> fieldNames = grid[0];
            List<string> fieldTypes = grid[1];

            int colCount = Math.Min(fieldNames.Count, fieldTypes.Count);
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("{");
            sb.AppendLine("  \"data\": [");

            int dataRowCount = 0;
            for (int r = 2; r < grid.Count; r++)
            {
                List<string> row = grid[r];
                if (row == null || IsRowEmpty(row))
                {
                    continue;
                }

                if (dataRowCount > 0)
                {
                    sb.AppendLine(",");
                }

                sb.AppendLine("    {");
                int validFieldCount = 0;

                for (int c = 0; c < colCount; c++)
                {
                    string fName = fieldNames[c] != null ? fieldNames[c].Trim() : string.Empty;
                    string fType = fieldTypes[c] != null ? fieldTypes[c].Trim() : string.Empty;

                    if (string.IsNullOrEmpty(fName) || string.IsNullOrEmpty(fType))
                    {
                        continue;
                    }

                    string rawVal = c < row.Count ? row[c] : string.Empty;
                    object convertedVal = TableValueConverter.ConvertValue(rawVal, fType, fName, r + 1, fileName);

                    if (validFieldCount > 0)
                    {
                        sb.AppendLine(",");
                    }

                    string jsonKey = ToCamelCase(fName);
                    string jsonValue = FormatJsonValue(convertedVal, fType);

                    sb.Append($"      \"{jsonKey}\": {jsonValue}");
                    validFieldCount++;
                }

                sb.AppendLine();
                sb.Append("    }");
                dataRowCount++;
            }

            sb.AppendLine();
            sb.AppendLine("  ]");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private static bool IsRowEmpty(List<string> row)
        {
            foreach (var cell in row)
            {
                if (!string.IsNullOrWhiteSpace(cell))
                    return false;
            }
            return true;
        }

        private static string ToCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str)) return str;
            if (char.IsLower(str[0])) return str;
            if (str.Length == 1) return str.ToLowerInvariant();
            return char.ToLowerInvariant(str[0]) + str.Substring(1);
        }

        private static string FormatJsonValue(object val, string fType)
        {
            if (val == null) return "null";

            string typeLower = fType != null ? fType.Trim().ToLowerInvariant() : "string";
            switch (typeLower)
            {
                case "bool":
                case "boolean":
                    return ((bool)val) ? "true" : "false";

                case "byte":
                case "int":
                case "int32":
                case "float":
                case "single":
                    return System.Convert.ToString(val, System.Globalization.CultureInfo.InvariantCulture);

                case "string":
                default:
                    string str = val.ToString();
                    str = str.Replace("\\", "\\\\")
                             .Replace("\"", "\\\"")
                             .Replace("\n", "\\n")
                             .Replace("\r", "\\r")
                             .Replace("\t", "\\t");
                    return $"\"{str}\"";
            }
        }
    }
}
