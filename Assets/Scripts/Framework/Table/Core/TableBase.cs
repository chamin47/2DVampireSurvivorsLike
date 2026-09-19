using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Table
{
    /// <summary>
    /// 테이블 데이터를 관리하는 제네릭 추상 기반 클래스입니다.
    /// Dictionary 기반 빠른 검색($O(1)$) 및 데이터 조회를 제공합니다.
    /// </summary>
    /// <typeparam name="TKey">ID 식별자 타입 (기본 int)</typeparam>
    /// <typeparam name="TData">행 데이터 타입 (ITableData 구현체)</typeparam>
    [Serializable]
    public abstract class TableBase<TKey, TData> : ITable, ISerializationCallbackReceiver
        where TData : ITableData<TKey>
    {
        [SerializeField]
        protected List<TData> dataList = new List<TData>();

        protected readonly Dictionary<TKey, TData> dataMap = new Dictionary<TKey, TData>();

        /// <summary>
        /// 테이블에 들어있는 데이터 항목 개수입니다.
        /// </summary>
        public int Count => dataList != null ? dataList.Count : 0;

        /// <summary>
        /// 데이터를 리스트에서 딕셔너리로 인덱싱하여 빠른 조회가 가능하도록 초기화합니다.
        /// </summary>
        public virtual void Initialize()
        {
            dataMap.Clear();
            if (dataList == null) return;

            foreach (var item in dataList)
            {
                if (item == null) continue;

                TKey key = item.Id;
                if (dataMap.ContainsKey(key))
                {
                    Debug.LogWarning($"[{GetType().Name}] Duplicate Key found: '{key}'. Skipping duplicate entry.");
                    continue;
                }
                dataMap.Add(key, item);
            }
        }

        /// <summary>
        /// Key로 데이터를 검색합니다. 존재하지 않을 경우 default(TData) 반환 및 로그 출력.
        /// </summary>
        public TData Get(TKey key)
        {
            if (dataMap.TryGetValue(key, out var data))
            {
                return data;
            }
            Debug.LogError($"[{GetType().Name}] Key '{key}' was not found in table.");
            return default;
        }

        /// <summary>
        /// Key로 안전하게 데이터를 검색합니다.
        /// </summary>
        public bool TryGet(TKey key, out TData data)
        {
            return dataMap.TryGetValue(key, out data);
        }

        /// <summary>
        /// 해당 Key의 데이터 존재 여부를 확인합니다.
        /// </summary>
        public bool Contains(TKey key)
        {
            return dataMap.ContainsKey(key);
        }

        /// <summary>
        /// 모든 데이터 항목 리스트를 읽기 전용으로 반환합니다.
        /// </summary>
        public IReadOnlyList<TData> GetAll()
        {
            return dataList;
        }

        /// <summary>
        /// 파싱 완료 후 데이터 리스트를 설정하고 맵을 구축합니다.
        /// </summary>
        public void SetDataList(List<TData> newList)
        {
            dataList = newList ?? new List<TData>();
            Initialize();
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            Initialize();
        }
    }

    /// <summary>
    /// int 키를 사용하는 표준 TableBase 기본 클래스입니다.
    /// </summary>
    [Serializable]
    public abstract class TableBase<TData> : TableBase<int, TData>
        where TData : ITableData<int>
    {
    }
}
