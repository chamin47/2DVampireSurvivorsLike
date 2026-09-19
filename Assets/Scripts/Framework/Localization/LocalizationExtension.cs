using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

public static class LocalizationExtension
{
    public enum TableLocalization
    {
        DefaultString,
    }

    /// <summary>
    /// 문자열 키를 사용하여 Localization 테이블에서 현재 설정된 로케일의 문자열을 반환합니다.
    /// Localization 시스템이 초기화되어 있고 해당 테이블/엔트리가 존재하면 번역된 문자열을 반환하며,
    /// 초기화되지 않았거나 엔트리를 찾지 못한 경우 키(key)를 그대로 반환합니다.
    /// </summary>
    public static string ToLocalizedString(this string key)
    {
        return ToLocalizedString(key, TableLocalization.DefaultString);
    }

    /// <summary>
    /// 문자열 키와 TableLocalization 열거형 테이블을 사용하여 문자열을 반환합니다.
    /// </summary>
    public static string ToLocalizedString(this string key, TableLocalization table)
    {
        return ToLocalizedString(key, table.ToString());
    }

    /// <summary>
    /// 문자열 키와 테이블 이름을 사용하여 Localization 테이블에서 문자열을 반환합니다.
    /// </summary>
    public static string ToLocalizedString(this string key, string tableName)
    {
        if (string.IsNullOrEmpty(key))
            return string.Empty;

        if (string.IsNullOrEmpty(tableName))
            tableName = TableLocalization.DefaultString.ToString();

        var initOp = LocalizationSettings.InitializationOperation;
        if (initOp.IsValid() && initOp.IsDone)
        {
            try
            {
                // 1. 테이블이 이미 로드되어 있는 경우 엔트리 직접 조회
                var stringTable = LocalizationSettings.StringDatabase.GetTable(tableName);
                if (stringTable != null)
                {
                    var entry = stringTable.GetEntry(key);
                    if (entry != null)
                    {
                        return entry.LocalizedValue ?? entry.Value ?? key;
                    }
                }

                // 2. 동기적 번역 문자열 반환 시도
                var localizedString = LocalizationSettings.StringDatabase.GetLocalizedString(tableName, key);
                if (!string.IsNullOrEmpty(localizedString))
                {
                    return localizedString;
                }
            }
            catch
            {
                // 예외 발생 시 fallback 처리
            }
        }

        // 초기화 전 또는 로드되지 않은 경우 안전하게 키 반환 (fallback)
        return key;
    }
}