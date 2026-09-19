using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Asset
{
    /// <summary>
    /// 로드된 AssetHandle을 Key 및 Type별로 캐싱하고 조회/타입 검증/삭제를 담당하는 캐시 관리자 클래스입니다.
    /// </summary>
    public class AssetCache
    {
        private readonly Dictionary<string, AssetHandle> cacheMap = new Dictionary<string, AssetHandle>();

        /// <summary>
        /// 캐시에 Asset이 존재하는지 확인합니다.
        /// </summary>
        public bool Contains(string key)
        {
            return cacheMap.ContainsKey(key);
        }

        /// <summary>
        /// 캐시된 AssetHandle을 가져옵니다.
        /// </summary>
        public bool TryGetHandle(string key, out AssetHandle handle)
        {
            return cacheMap.TryGetValue(key, out handle);
        }

        /// <summary>
        /// 캐시에서 요청한 타입의 Asset을 가져옵니다.
        /// 타입 불일치 시 명확한 경고/오류 로그를 출력합니다.
        /// </summary>
        public bool TryGet<T>(string key, out T asset, out AssetHandle handle) where T : UnityEngine.Object
        {
            asset = null;
            handle = null;

            if (!cacheMap.TryGetValue(key, out var existingHandle))
            {
                return false;
            }

            handle = existingHandle;

            if (existingHandle.Asset is T typedAsset)
            {
                asset = typedAsset;
                return true;
            }

            Type requestedType = typeof(T);
            Type actualType = existingHandle.Asset != null ? existingHandle.Asset.GetType() : existingHandle.AssetType;

            Debug.LogError($"[AssetCache] Type Mismatch for Key '{key}'. Requested: {requestedType.Name}, Actual Cached Type: {actualType?.Name}");
            return false;
        }

        /// <summary>
        /// 새로운 AssetHandle을 캐시에 등록합니다.
        /// </summary>
        public void Add(AssetHandle handle)
        {
            if (handle == null || string.IsNullOrEmpty(handle.Key))
            {
                Debug.LogWarning("[AssetCache] Cannot add null or empty key handle to cache.");
                return;
            }

            if (cacheMap.ContainsKey(handle.Key))
            {
                Debug.LogWarning($"[AssetCache] Key '{handle.Key}' already exists in cache. Incrementing ref count instead.");
                cacheMap[handle.Key].IncrementRef();
                return;
            }

            cacheMap[handle.Key] = handle;
        }

        /// <summary>
        /// 특정 Key의 AssetHandle을 캐시에서 제거합니다.
        /// </summary>
        public bool Remove(string key, out AssetHandle handle)
        {
            if (cacheMap.TryGetValue(key, out handle))
            {
                cacheMap.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 캐시된 모든 AssetHandle 목록을 반환합니다.
        /// </summary>
        public List<AssetHandle> GetAllHandles()
        {
            return new List<AssetHandle>(cacheMap.Values);
        }

        /// <summary>
        /// 모든 캐시를 비웁니다.
        /// </summary>
        public void Clear(Action<AssetHandle> onRelease)
        {
            foreach (var handle in cacheMap.Values)
            {
                onRelease?.Invoke(handle);
            }
            cacheMap.Clear();
        }
    }
}
