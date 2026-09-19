using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Framework.Asset
{
    /// <summary>
    /// GameObject의 재사용(Spawn/Despawn)을 관리하고 원본 Asset 생명주기와 동기화하는 Object Pool 클래스입니다.
    /// </summary>
    public class AssetPool
    {
        private readonly Transform poolRoot;
        // Key별 비활성화 상태의 객체 큐
        private readonly Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
        // Instance -> Key 매핑 (어떤 Key에서 Spawn되었는지 추적)
        private readonly Dictionary<GameObject, string> instanceKeyMap = new Dictionary<GameObject, string>();
        // Key별 현재 생존 중인 총 Instance 수 (Active + Inactive in Pool)
        private readonly Dictionary<string, int> totalInstanceCountMap = new Dictionary<string, int>();
        // Addressables로 Instantiate되었는지 추적하는 집합
        private readonly HashSet<GameObject> addressableInstances = new HashSet<GameObject>();

        public AssetPool(Transform poolRoot)
        {
            this.poolRoot = poolRoot;
        }

        /// <summary>
        /// 해당 Key로 관리되는 Pool에 활성 또는 비활성 객체가 존재하는지 확인합니다.
        /// (원본 Asset Unload를 방지하는 용도로 사용)
        /// </summary>
        public bool HasActiveOrPooledInstances(string key)
        {
            return totalInstanceCountMap.TryGetValue(key, out int count) && count > 0;
        }

        /// <summary>
        /// 주어진 객체가 Pool에 의해 생성 및 관리되는 객체인지 확인합니다.
        /// </summary>
        public bool IsPooledInstance(GameObject instance)
        {
            return instance != null && instanceKeyMap.ContainsKey(instance);
        }

        /// <summary>
        /// Addressables에서 Instantiate된 객체인지 확인합니다.
        /// </summary>
        public bool IsAddressableInstance(GameObject instance)
        {
            return instance != null && addressableInstances.Contains(instance);
        }

        /// <summary>
        /// Addressables 인스턴스 등록
        /// </summary>
        public void RegisterAddressableInstance(GameObject instance)
        {
            if (instance != null)
            {
                addressableInstances.Add(instance);
            }
        }

        /// <summary>
        /// Pool에서 객체를 꺼내거나 새 인스턴스를 생성하여 반환합니다.
        /// </summary>
        public GameObject Spawn(string key, Func<GameObject> createFunc, Transform parent = null, bool isAddressable = false)
        {
            GameObject obj = null;

            if (poolDictionary.TryGetValue(key, out var queue) && queue.Count > 0)
            {
                obj = queue.Dequeue();
                while (obj == null && queue.Count > 0)
                {
                    obj = queue.Dequeue(); // 파괴된 널 인스턴스 스킵
                }
            }

            if (obj == null)
            {
                if (createFunc == null)
                {
                    Debug.LogError($"[AssetPool] Spawn failed for Key '{key}'. Create delegate is null.");
                    return null;
                }

                obj = createFunc.Invoke();
                if (obj == null)
                {
                    Debug.LogError($"[AssetPool] Failed to instantiate object for Key '{key}'.");
                    return null;
                }

                instanceKeyMap[obj] = key;
                if (!totalInstanceCountMap.ContainsKey(key))
                {
                    totalInstanceCountMap[key] = 0;
                }
                totalInstanceCountMap[key]++;

                if (isAddressable)
                {
                    addressableInstances.Add(obj);
                }
            }

            if (parent != null)
            {
                obj.transform.SetParent(parent, false);
            }
            else
            {
                obj.transform.SetParent(null, false);
            }

            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// 사용 완료된 객체를 Pool에 반환하고 비활성화합니다.
        /// </summary>
        public bool Despawn(GameObject instance)
        {
            if (instance == null)
            {
                Debug.LogWarning("[AssetPool] Despawn requested for null instance.");
                return false;
            }

            if (!instanceKeyMap.TryGetValue(instance, out string key))
            {
                Debug.LogWarning($"[AssetPool] Instance '{instance.name}' is not registered in Pool system.");
                return false;
            }

            if (!poolDictionary.TryGetValue(key, out var queue))
            {
                queue = new Queue<GameObject>();
                poolDictionary[key] = queue;
            }

            if (queue.Contains(instance))
            {
                Debug.LogWarning($"[AssetPool] Instance '{instance.name}' is already despawned in pool.");
                return false;
            }

            instance.SetActive(false);
            if (poolRoot != null)
            {
                instance.transform.SetParent(poolRoot, false);
            }

            queue.Enqueue(instance);
            return true;
        }

        /// <summary>
        /// 특정 Key의 모든 Pool 객체(비활성화 상태)를 파괴 및 정리합니다.
        /// </summary>
        public void ClearPool(string key)
        {
            if (poolDictionary.TryGetValue(key, out var queue))
            {
                while (queue.Count > 0)
                {
                    var obj = queue.Dequeue();
                    DestroyInstance(obj);
                }
                poolDictionary.Remove(key);
            }
        }

        /// <summary>
        /// 등록된 모든 Pool 객체를 완전히 파괴하고 초기화합니다.
        /// </summary>
        public void ClearAll()
        {
            foreach (var kvp in poolDictionary)
            {
                var queue = kvp.Value;
                while (queue.Count > 0)
                {
                    var obj = queue.Dequeue();
                    DestroyInstance(obj);
                }
            }

            poolDictionary.Clear();
            instanceKeyMap.Clear();
            totalInstanceCountMap.Clear();
            addressableInstances.Clear();
        }

        /// <summary>
        /// 단일 인스턴스 파괴 및 카운트 정리
        /// </summary>
        public void DestroyInstance(GameObject instance)
        {
            if (instance == null) return;

            if (instanceKeyMap.TryGetValue(instance, out string key))
            {
                instanceKeyMap.Remove(instance);
                if (totalInstanceCountMap.ContainsKey(key))
                {
                    totalInstanceCountMap[key] = Math.Max(0, totalInstanceCountMap[key] - 1);
                }
            }

            if (addressableInstances.Contains(instance))
            {
                addressableInstances.Remove(instance);
                Addressables.ReleaseInstance(instance);
            }
            else
            {
                UnityEngine.Object.Destroy(instance);
            }
        }
    }
}
