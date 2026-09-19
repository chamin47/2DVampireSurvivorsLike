using System;
using System.Globalization;
using UnityEngine;

namespace Framework.Table
{
    /// <summary>
    /// Excel/CSV 텍스트 셀 데이터를 C# 기본 자료형(byte, int, float, bool, string)으로 안전하게 변환하는 유틸리티 클래스입니다.
    /// 잘못된 데이터 입력 시 원인과 위치를 알 수 있도록 오류 로그를 출력합니다.
    /// </summary>
    public static class TableValueConverter
    {
        /// <summary>
        /// 문자열 데이터를 목표 자료형으로 변환합니다.
        /// </summary>
        /// <param name="rawValue">셀 원본 문자열</param>
        /// <param name="targetTypeName">목표 타입 이름 ("byte", "int", "float", "bool", "string")</param>
        /// <param name="columnName">오류 리포팅용 컬럼 헤더 이름</param>
        /// <param name="rowIndex">오류 리포팅용 행 번호</param>
        /// <param name="fileName">오류 리포팅용 파일 이름</param>
        /// <returns>변환된 데이터 객체 (박싱)</returns>
        public static object ConvertValue(string rawValue, string targetTypeName, string columnName = "", int rowIndex = -1, string fileName = "")
        {
            string trimmed = rawValue != null ? rawValue.Trim() : string.Empty;
            string typeLower = targetTypeName != null ? targetTypeName.Trim().ToLowerInvariant() : "string";

            switch (typeLower)
            {
                case "byte":
                    if (byte.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out byte byteRes))
                    {
                        return byteRes;
                    }
                    LogError("byte", rawValue, columnName, rowIndex, fileName, "Expected byte value (0 ~ 255).");
                    return (byte)0;

                case "int":
                case "int32":
                    if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intRes))
                    {
                        return intRes;
                    }
                    LogError("int", rawValue, columnName, rowIndex, fileName, "Expected valid integer number.");
                    return 0;

                case "float":
                case "single":
                    if (float.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatRes))
                    {
                        return floatRes;
                    }
                    LogError("float", rawValue, columnName, rowIndex, fileName, "Expected valid float decimal number.");
                    return 0f;

                case "bool":
                case "boolean":
                    if (bool.TryParse(trimmed, out bool boolRes))
                    {
                        return boolRes;
                    }
                    if (trimmed == "1" || trimmed.Equals("true", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (trimmed == "0" || trimmed.Equals("false", StringComparison.OrdinalIgnoreCase) || trimmed.Equals("n", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    LogError("bool", rawValue, columnName, rowIndex, fileName, "Expected 'true', 'false', '1', or '0'.");
                    return false;

                case "string":
                default:
                    return rawValue ?? string.Empty;
            }
        }

        private static void LogError(string targetType, string rawValue, string columnName, int rowIndex, string fileName, string reason)
        {
            string location = string.IsNullOrEmpty(fileName) ? "" : $" File: '{fileName}'";
            if (rowIndex >= 0) location += $", Row: {rowIndex}";
            if (!string.IsNullOrEmpty(columnName)) location += $", Column: '{columnName}'";

            Debug.LogError($"[TableValueConverter] Parsing failed for target type '{targetType}' with raw input '{rawValue}'. {reason}{location}");
        }
    }
}
