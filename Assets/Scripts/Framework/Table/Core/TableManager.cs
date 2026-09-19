using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Table
{
    /// <summary>
    /// 게임 실행 시 JSON 데이터 로드, 캐싱, 테이블 조회를 담당하는 중앙 테이블 관리자입니다.
    /// 싱글톤 남용 없이 static API (TableManager.Get<TTable>()) 및 런타임 캐싱을 제공합니다.
    /// </summary>
    public class TableManager
    {
        private static TableManager instance;

        public static TableManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TableManager();
                }
                return instance;
            }
        }

        private readonly Dictionary<Type, ITable> tableCache = new Dictionary<Type, ITable>();

        /// <summary>
        /// 특정 테이블 객체를 반환합니다. 이미 캐싱되어 있다면 캐시에서 반환하고,
        /// 없으면 Resources 디렉토리에서 JSON을 로드하여 반환합니다.
        /// </summary>
        /// <example>
        /// CharacterTable table = TableManager.Get<CharacterTable>();
        /// CharacterData character = TableManager.Get<CharacterTable>().Get(1001);
        /// </example>
        public static TTable Get<TTable>() where TTable : class, ITable, new()
        {
            return Instance.GetTableInternal<TTable>();
        }

        /// <summary>
        /// 지정한 타입의 테이블을 캐시에서 가져오거나 새로 로드합니다.
        /// </summary>
        public TTable GetTableInternal<TTable>() where TTable : class, ITable, new()
        {
            Type tableType = typeof(TTable);

            if (tableCache.TryGetValue(tableType, out var cachedTable))
            {
                return cachedTable as TTable;
            }

            TTable table = LoadTable<TTable>();
            if (table != null)
            {
                tableCache[tableType] = table;
            }

            return table;
        }

        /// <summary>
        /// JSON 문자열을 직접 전달하여 테이블을 수동 등록/업데이트합니다.
        /// </summary>
        public static void RegisterTable<TTable>(TTable table) where TTable : class, ITable
        {
            if (table == null) return;
            Instance.tableCache[typeof(TTable)] = table;
        }

        /// <summary>
        /// 모든 캐시된 테이블을 비웁니다.
        /// </summary>
        public static void Clear()
        {
            Instance.tableCache.Clear();
        }

        /// <summary>
        /// 테이블 타입 정보를 기반으로 Resources/Tables/ 경로에서 JSON 파일을 찾아 로드합니다.
        /// </summary>
        private TTable LoadTable<TTable>() where TTable : class, ITable, new()
        {
            Type tableType = typeof(TTable);
            string tableName = tableType.Name;

            // 시도할 파일명 후보 리스트: e.g. "CharacterTable", "CharacterData", "character"
            List<string> candidateNames = new List<string>
            {
                tableName,
                tableName.EndsWith("Table") ? tableName.Substring(0, tableName.Length - 5) : tableName,
                tableName.EndsWith("Table") ? tableName.Substring(0, tableName.Length - 5) + "Data" : tableName + "Data",
                tableName.ToLowerInvariant()
            };

            TextAsset jsonAsset = null;
            string matchedPath = string.Empty;

            foreach (var name in candidateNames)
            {
                string path = $"Tables/{name}";
                jsonAsset = Resources.Load<TextAsset>(path);
                if (jsonAsset != null)
                {
                    matchedPath = path;
                    break;
                }
            }

            if (jsonAsset == null)
            {
                Debug.LogError($"[TableManager] Failed to find table JSON asset for type '{tableName}'. Checked candidates under Resources/Tables/.");
                return null;
            }

            TTable tableInstance = new TTable();

            // Reflection으로 TableBase의 PopulateTable 호출
            bool parseSuccess = InvokePopulate(tableInstance, jsonAsset.text, matchedPath);
            if (!parseSuccess)
            {
                Debug.LogError($"[TableManager] Failed to parse JSON table '{tableName}' from path 'Resources/{matchedPath}'.");
                return null;
            }

            Debug.Log($"[TableManager] Successfully loaded table '{tableName}' ({tableInstance.Count} rows) from 'Resources/{matchedPath}'.");
            return tableInstance;
        }

        private bool InvokePopulate(ITable tableInstance, string jsonText, string fileName)
        {
            Type baseType = tableInstance.GetType().BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(TableBase<,>))
                {
                    Type[] genericArgs = baseType.GetGenericArguments();
                    Type keyType = genericArgs[0];
                    Type dataType = genericArgs[1];

                    var method = typeof(TableJsonParser).GetMethod(nameof(TableJsonParser.PopulateTable))
                        .MakeGenericMethod(tableInstance.GetType(), keyType, dataType);

                    var result = method.Invoke(null, new object[] { tableInstance, jsonText, fileName });
                    return result is bool b && b;
                }
                baseType = baseType.BaseType;
            }

            Debug.LogError($"[TableManager] Table '{tableInstance.GetType().Name}' does not inherit from TableBase<TKey, TData>.");
            return false;
        }
    }
}
