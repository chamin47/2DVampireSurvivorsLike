using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Table
{
    /// <summary>
    /// JsonUtility 직렬화용 Generic Wrapper 구조 클래스입니다.
    /// </summary>
    [Serializable]
    internal class TableJsonWrapper<T>
    {
        public List<T> data;
    }

    /// <summary>
    /// JSON 데이터를 C# 데이터 및 테이블 객체로 파싱하는 파서 클래스입니다.
    /// </summary>
    public static class TableJsonParser
    {
        /// <summary>
        /// JSON 문자열을 파싱하여 TData 리스트 목록을 반환합니다.
        /// Expected JSON format: { "data": [ { ... }, { ... } ] }
        /// </summary>
        public static List<TData> FromJsonList<TData>(string jsonText, string fileName = "")
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                Debug.LogError($"[TableJsonParser] JSON content is null or empty. (File: '{fileName}')");
                return new List<TData>();
            }

            try
            {
                var wrapper = JsonUtility.FromJson<TableJsonWrapper<TData>>(jsonText);
                if (wrapper != null && wrapper.data != null)
                {
                    return wrapper.data;
                }
                Debug.LogWarning($"[TableJsonParser] No 'data' list array found in JSON. (File: '{fileName}')");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TableJsonParser] Failed to parse JSON to List<{typeof(TData).Name}>. File: '{fileName}'. Error: {ex.Message}");
            }

            return new List<TData>();
        }

        /// <summary>
        /// JSON 문자열을 파싱하여 새 TTable 인스턴스를 생성하고 리턴합니다.
        /// </summary>
        public static TTable FromJsonTable<TTable, TKey, TData>(string jsonText, string fileName = "")
            where TTable : TableBase<TKey, TData>, new()
            where TData : ITableData<TKey>
        {
            var table = new TTable();
            PopulateTable<TTable, TKey, TData>(table, jsonText, fileName);
            return table;
        }

        /// <summary>
        /// JSON 문자열을 파싱하여 int 키 기준의 새 TTable 인스턴스를 생성하고 리턴합니다.
        /// </summary>
        public static TTable FromJsonTable<TTable, TData>(string jsonText, string fileName = "")
            where TTable : TableBase<int, TData>, new()
            where TData : ITableData<int>
        {
            return FromJsonTable<TTable, int, TData>(jsonText, fileName);
        }

        /// <summary>
        /// 기존 TTable 인스턴스에 JSON 파싱 결과를 로드합니다.
        /// </summary>
        public static bool PopulateTable<TTable, TKey, TData>(TTable table, string jsonText, string fileName = "")
            where TTable : TableBase<TKey, TData>
            where TData : ITableData<TKey>
        {
            if (table == null)
            {
                Debug.LogError($"[TableJsonParser] Target table instance is null. (File: '{fileName}')");
                return false;
            }

            var list = FromJsonList<TData>(jsonText, fileName);
            table.SetDataList(list);
            return true;
        }
    }
}
