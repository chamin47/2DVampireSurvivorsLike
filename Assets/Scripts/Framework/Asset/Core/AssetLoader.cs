using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Framework.Asset
{
    /// <summary>
    /// Resources 및 Addressables 기반의 Asset 로드, 캐싱, Instantiate, Component 반환, Sprite Atlas 및 Object Pool 관리를 총괄하는 전역 Singleton/Static API 시스템입니다.
    /// </summary>
    public class AssetLoader : MonoBehaviour
    {
        private static AssetLoader instance;

        /// <summary>
        /// AssetLoader 전역 Singleton 인스턴스입니다.
        /// </summary>
        public static AssetLoader Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<AssetLoader>();
                    if (instance == null)
                    {
                        var go = new GameObject("[AssetLoader]");
                        instance = go.AddComponent<AssetLoader>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        private readonly AssetCache assetCache = new AssetCache();
        private readonly Dictionary<string, AssetDefinition> definitions = new Dictionary<string, AssetDefinition>();
        private readonly Dictionary<(string atlasKey, string spriteName), Sprite> spriteCache = new Dictionary<(string, string), Sprite>();
        private AssetPool assetPool;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePool();
        }

        private void InitializePool()
        {
            var poolRootGo = transform.Find("[AssetPool]");
            if (poolRootGo == null)
            {
                var go = new GameObject("[AssetPool]");
                go.transform.SetParent(transform, false);
                poolRootGo = go.transform;
            }

            assetPool = new AssetPool(poolRootGo);
        }

        #region Definition Management

        /// <summary>
        /// Asset 식별 Key와 주소/출처 매핑 정의를 등록합니다.
        /// </summary>
        public static void RegisterDefinition(AssetDefinition definition)
        {
            if (definition != null && !string.IsNullOrEmpty(definition.key))
            {
                Instance.definitions[definition.key] = definition;
            }
        }

        /// <summary>
        /// 복수 Asset 정의를 등록합니다.
        /// </summary>
        public static void RegisterDefinitions(IEnumerable<AssetDefinition> definitionList)
        {
            if (definitionList == null) return;
            foreach (var def in definitionList)
            {
                RegisterDefinition(def);
            }
        }

        #endregion

        #region Synchronous Asset Loading

        /// <summary>
        /// Key 기반으로 Asset을 동기 로드합니다. (캐시 우선 확인)
        /// </summary>
        public static T Load<T>(string key, AssetSource source = AssetSource.Auto) where T : UnityEngine.Object
        {
            return Instance.InternalLoad<T>(key, source);
        }

        private T InternalLoad<T>(string key, AssetSource source) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[AssetLoader] Load requested with null or empty key.");
                return null;
            }

            // 1. Check Asset Cache
            if (assetCache.TryGet<T>(key, out T cachedAsset, out var handle))
            {
                handle.IncrementRef();
                return cachedAsset;
            }

            // 2. Resolve Definition and Source
            ResolveKeyInfo(key, source, out string address, out AssetSource finalSource);

            T loadedAsset = null;
            AsyncOperationHandle addrHandle = default;
            bool isAddressable = false;

            try
            {
                if (finalSource == AssetSource.Addressables)
                {
                    var asyncHandle = Addressables.LoadAssetAsync<T>(address);
                    loadedAsset = asyncHandle.WaitForCompletion();
                    addrHandle = asyncHandle;
                    isAddressable = true;
                }
                else
                {
                    loadedAsset = Resources.Load<T>(address);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetLoader] Asset Load Exception. Key: {key}, Source: {finalSource}, Address: {address}\n{ex}");
            }

            if (loadedAsset == null)
            {
                Debug.LogError($"[AssetLoader] Asset Load Failed. Key: {key}, Source: {finalSource}, Address: {address}");
                return null;
            }

            // 3. Store in Cache
            AssetHandle newHandle = isAddressable
                ? new AssetHandle(key, typeof(T), finalSource, loadedAsset, addrHandle)
                : new AssetHandle(key, typeof(T), finalSource, loadedAsset);

            assetCache.Add(newHandle);
            return loadedAsset;
        }

        #endregion

        #region Asynchronous Asset Loading

        /// <summary>
        /// Key 기반으로 Asset을 비동기 로드합니다.
        /// </summary>
        public static Task<T> LoadAsync<T>(string key, AssetSource source = AssetSource.Auto) where T : UnityEngine.Object
        {
            return Instance.InternalLoadAsync<T>(key, source);
        }

        private async Task<T> InternalLoadAsync<T>(string key, AssetSource source) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[AssetLoader] LoadAsync requested with null or empty key.");
                return null;
            }

            // 1. Check Asset Cache
            if (assetCache.TryGet<T>(key, out T cachedAsset, out var handle))
            {
                handle.IncrementRef();
                return cachedAsset;
            }

            // 2. Resolve Definition and Source
            ResolveKeyInfo(key, source, out string address, out AssetSource finalSource);

            T loadedAsset = null;
            AsyncOperationHandle addrHandle = default;
            bool isAddressable = false;

            try
            {
                if (finalSource == AssetSource.Addressables)
                {
                    var asyncHandle = Addressables.LoadAssetAsync<T>(address);
                    loadedAsset = await asyncHandle.Task;
                    addrHandle = asyncHandle;
                    isAddressable = true;
                }
                else
                {
                    var request = Resources.LoadAsync<T>(address);
                    var tcs = new TaskCompletionSource<T>();
                    request.completed += _ => tcs.SetResult(request.asset as T);
                    loadedAsset = await tcs.Task;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetLoader] Async Asset Load Exception. Key: {key}, Source: {finalSource}, Address: {address}\n{ex}");
            }

            if (loadedAsset == null)
            {
                Debug.LogError($"[AssetLoader] Asset Load Failed. Key: {key}, Source: {finalSource}, Address: {address}");
                return null;
            }

            // 3. Store in Cache
            AssetHandle newHandle = isAddressable
                ? new AssetHandle(key, typeof(T), finalSource, loadedAsset, addrHandle)
                : new AssetHandle(key, typeof(T), finalSource, loadedAsset);

            assetCache.Add(newHandle);
            return loadedAsset;
        }

        #endregion

        #region GameObject Instantiate & Component Get

        /// <summary>
        /// Prefab Asset을 로드한 후 GameObject 인스턴스를 생성합니다.
        /// </summary>
        public static GameObject Instantiate(string key, Transform parent = null, AssetSource source = AssetSource.Auto)
        {
            var prefab = Load<GameObject>(key, source);
            if (prefab == null) return null;

            var instanceObj = UnityEngine.Object.Instantiate(prefab, parent);
            instanceObj.name = prefab.name;
            return instanceObj;
        }

        /// <summary>
        /// Prefab Asset을 로드한 후 위치 및 회전을 지정하여 GameObject 인스턴스를 생성합니다.
        /// </summary>
        public static GameObject Instantiate(string key, Vector3 position, Quaternion rotation, Transform parent = null, AssetSource source = AssetSource.Auto)
        {
            var prefab = Load<GameObject>(key, source);
            if (prefab == null) return null;

            var instanceObj = UnityEngine.Object.Instantiate(prefab, position, rotation, parent);
            instanceObj.name = prefab.name;
            return instanceObj;
        }

        /// <summary>
        /// Prefab Asset을 로드하여 Instantiate하고 지정한 Component를 반환합니다.
        /// </summary>
        public static T Instantiate<T>(string key, Transform parent = null, AssetSource source = AssetSource.Auto) where T : Component
        {
            var go = Instantiate(key, parent, source);
            if (go == null) return null;

            if (go.TryGetComponent<T>(out var component))
            {
                return component;
            }

            Debug.LogError($"[AssetLoader] Component '{typeof(T).Name}' not found on instantiated object for Key '{key}'!");
            return null;
        }

        /// <summary>
        /// Prefab Asset을 비동기로 로드한 후 GameObject 인스턴스를 생성합니다.
        /// </summary>
        public static async Task<GameObject> InstantiateAsync(string key, Transform parent = null, AssetSource source = AssetSource.Auto)
        {
            var prefab = await LoadAsync<GameObject>(key, source);
            if (prefab == null) return null;

            var instanceObj = UnityEngine.Object.Instantiate(prefab, parent);
            instanceObj.name = prefab.name;
            return instanceObj;
        }

        /// <summary>
        /// Prefab Asset을 비동기로 로드하고 Instantiate하여 Component를 반환합니다.
        /// </summary>
        public static async Task<T> InstantiateAsync<T>(string key, Transform parent = null, AssetSource source = AssetSource.Auto) where T : Component
        {
            var go = await InstantiateAsync(key, parent, source);
            if (go == null) return null;

            if (go.TryGetComponent<T>(out var component))
            {
                return component;
            }

            Debug.LogError($"[AssetLoader] Component '{typeof(T).Name}' not found on instantiated object for Key '{key}'!");
            return null;
        }

        #endregion

        #region Sprite Atlas & Sprite Cache

        /// <summary>
        /// Sprite Atlas를 동기로 로드하고 Atlas 내 특정 Sprite를 반환합니다.
        /// </summary>
        public static Sprite LoadSprite(string atlasKey, string spriteName, AssetSource source = AssetSource.Auto)
        {
            return Instance.InternalLoadSprite(atlasKey, spriteName, source);
        }

        private Sprite InternalLoadSprite(string atlasKey, string spriteName, AssetSource source)
        {
            var cacheKey = (atlasKey, spriteName);
            if (spriteCache.TryGetValue(cacheKey, out var cachedSprite) && cachedSprite != null)
            {
                return cachedSprite;
            }

            var atlas = Load<SpriteAtlas>(atlasKey, source);
            if (atlas == null)
            {
                Debug.LogError($"[AssetLoader] Sprite Not Found. Atlas: {atlasKey}, Sprite: {spriteName}");
                return null;
            }

            var sprite = atlas.GetSprite(spriteName);
            if (sprite == null)
            {
                Debug.LogError($"[AssetLoader] Sprite Not Found. Atlas: {atlasKey}, Sprite: {spriteName}");
                return null;
            }

            spriteCache[cacheKey] = sprite;
            return sprite;
        }

        /// <summary>
        /// Sprite Atlas를 비동기로 로드하고 Atlas 내 특정 Sprite를 반환합니다.
        /// </summary>
        public static async Task<Sprite> LoadSpriteAsync(string atlasKey, string spriteName, AssetSource source = AssetSource.Auto)
        {
            return await Instance.InternalLoadSpriteAsync(atlasKey, spriteName, source);
        }

        private async Task<Sprite> InternalLoadSpriteAsync(string atlasKey, string spriteName, AssetSource source)
        {
            var cacheKey = (atlasKey, spriteName);
            if (spriteCache.TryGetValue(cacheKey, out var cachedSprite) && cachedSprite != null)
            {
                return cachedSprite;
            }

            var atlas = await LoadAsync<SpriteAtlas>(atlasKey, source);
            if (atlas == null)
            {
                Debug.LogError($"[AssetLoader] Sprite Not Found. Atlas: {atlasKey}, Sprite: {spriteName}");
                return null;
            }

            var sprite = atlas.GetSprite(spriteName);
            if (sprite == null)
            {
                Debug.LogError($"[AssetLoader] Sprite Not Found. Atlas: {atlasKey}, Sprite: {spriteName}");
                return null;
            }

            spriteCache[cacheKey] = sprite;
            return sprite;
        }

        #endregion

        #region Object Pooling (Spawn / Despawn)

        /// <summary>
        /// Pool에서 GameObject 인스턴스를 가져오거나 새로 생성하여 반환합니다.
        /// </summary>
        public static GameObject Spawn(string key, Transform parent = null, AssetSource source = AssetSource.Auto)
        {
            return Instance.InternalSpawn(key, parent, source);
        }

        /// <summary>
        /// Pool에서 GameObject 인스턴스를 가져오고 Component를 반환합니다.
        /// </summary>
        public static T Spawn<T>(string key, Transform parent = null, AssetSource source = AssetSource.Auto) where T : Component
        {
            var go = Spawn(key, parent, source);
            if (go == null) return null;

            if (go.TryGetComponent<T>(out var component))
            {
                return component;
            }

            Debug.LogError($"[AssetLoader] Component '{typeof(T).Name}' not found on spawned object for Key '{key}'!");
            return null;
        }

        private GameObject InternalSpawn(string key, Transform parent, AssetSource source)
        {
            if (assetPool == null) InitializePool();

            ResolveKeyInfo(key, source, out _, out AssetSource finalSource);
            bool isAddressable = (finalSource == AssetSource.Addressables);

            return assetPool.Spawn(key, () =>
            {
                var prefab = InternalLoad<GameObject>(key, source);
                if (prefab == null) return null;
                var inst = UnityEngine.Object.Instantiate(prefab);
                inst.name = prefab.name;
                return inst;
            }, parent, isAddressable);
        }

        /// <summary>
        /// 사용 완료된 Pooled GameObject를 Pool로 반환합니다.
        /// </summary>
        public static void Despawn(GameObject instanceObj)
        {
            if (Instance.assetPool == null) return;
            Instance.assetPool.Despawn(instanceObj);
        }

        #endregion

        #region Asset Unload & Release

        /// <summary>
        /// 더 이상 사용하지 않는 Asset을 Unload합니다.
        /// (Pool에 해당 Asset의 인스턴스가 남아 있으면 Unload를 거부하고 안전하게 유지)
        /// </summary>
        public static void Unload(string key)
        {
            Instance.InternalUnload(key);
        }

        private void InternalUnload(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            // Pool 인스턴스 생존 여부 검사
            if (assetPool != null && assetPool.HasActiveOrPooledInstances(key))
            {
                Debug.LogWarning($"[AssetLoader] Cannot unload asset '{key}' because active or pooled instances still exist in AssetPool.");
                return;
            }

            if (!assetCache.TryGetHandle(key, out var handle))
            {
                return;
            }

            int remainingRef = handle.DecrementRef();
            if (remainingRef <= 0)
            {
                if (assetCache.Remove(key, out var removedHandle))
                {
                    removedHandle.ReleaseAsset();
                    Debug.Log($"[AssetLoader] Asset '{key}' successfully unloaded.");
                }
            }
        }

        /// <summary>
        /// Instantiate되거나 Spawn된 GameObject 인스턴스를 안전하게 해제합니다.
        /// (Pooled 인스턴스: Despawn / Addressable 인스턴스: Addressables.ReleaseInstance / 일반: Destroy)
        /// </summary>
        public static void Release(GameObject instanceObj)
        {
            if (instanceObj == null) return;

            if (Instance.assetPool != null && Instance.assetPool.IsPooledInstance(instanceObj))
            {
                Instance.assetPool.Despawn(instanceObj);
            }
            else if (Instance.assetPool != null && Instance.assetPool.IsAddressableInstance(instanceObj))
            {
                Instance.assetPool.DestroyInstance(instanceObj);
            }
            else
            {
                UnityEngine.Object.Destroy(instanceObj);
            }
        }

        /// <summary>
        /// AssetLoader가 소유하고 관리하는 모든 Asset, Pool 및 캐시를 완전히 해제합니다.
        /// </summary>
        public static void ReleaseAll()
        {
            Instance.InternalReleaseAll();
        }

        private void InternalReleaseAll()
        {
            if (assetPool != null)
            {
                assetPool.ClearAll();
            }

            spriteCache.Clear();

            assetCache.Clear(handle =>
            {
                handle.ReleaseAsset();
            });

            Resources.UnloadUnusedAssets();
            Debug.Log("[AssetLoader] All assets, pools, and caches have been released.");
        }

        /// <summary>
        /// 씬 전환 시 사용하지 않는 Asset 정리 (초기 구현은 ReleaseAll 호출)
        /// </summary>
        public static void ReleaseSceneAssets()
        {
            ReleaseAll();
        }

        #endregion

        #region Key Resolution Helper

        private void ResolveKeyInfo(string key, AssetSource requestedSource, out string address, out AssetSource finalSource)
        {
            if (definitions.TryGetValue(key, out var def))
            {
                address = string.IsNullOrEmpty(def.address) ? key : def.address;
                finalSource = (requestedSource != AssetSource.Auto) ? requestedSource : def.source;
            }
            else
            {
                address = key;
                finalSource = requestedSource;
            }

            if (finalSource == AssetSource.Auto)
            {
                finalSource = AssetSource.Resources;
            }
        }

        #endregion
    }
}
