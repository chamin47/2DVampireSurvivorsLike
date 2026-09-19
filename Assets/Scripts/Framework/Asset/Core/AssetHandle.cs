using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Framework.Asset
{
    /// <summary>
    /// 로드된 Asset의 참조 정보, 로딩 방식, Addressables Handle 및 RefCount를 관리하는 래퍼 클래스입니다.
    /// </summary>
    public class AssetHandle
    {
        /// <summary>
        /// Asset 식별 Key
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// 로드된 Asset의 실제 Type
        /// </summary>
        public Type AssetType { get; private set; }

        /// <summary>
        /// Asset 로드 출처 (Resources / Addressables)
        /// </summary>
        public AssetSource Source { get; private set; }

        /// <summary>
        /// 로드된 Unity Object 참조
        /// </summary>
        public UnityEngine.Object Asset { get; private set; }

        /// <summary>
        /// Addressables 비동기 작업 핸들
        /// </summary>
        public AsyncOperationHandle AddressableHandle { get; private set; }

        /// <summary>
        /// Addressables Handle 보유 여부
        /// </summary>
        public bool HasAddressableHandle { get; private set; }

        /// <summary>
        /// 현재 참조 카운트
        /// </summary>
        public int RefCount { get; private set; }

        public AssetHandle(string key, Type assetType, AssetSource source, UnityEngine.Object asset)
        {
            Key = key;
            AssetType = assetType;
            Source = source;
            Asset = asset;
            HasAddressableHandle = false;
            RefCount = 1;
        }

        public AssetHandle(string key, Type assetType, AssetSource source, UnityEngine.Object asset, AsyncOperationHandle handle)
        {
            Key = key;
            AssetType = assetType;
            Source = source;
            Asset = asset;
            AddressableHandle = handle;
            HasAddressableHandle = true;
            RefCount = 1;
        }

        public void IncrementRef()
        {
            RefCount++;
        }

        public int DecrementRef()
        {
            RefCount = Math.Max(0, RefCount - 1);
            return RefCount;
        }

        /// <summary>
        /// 실제 Asset 자원을 해제합니다. Addressables인 경우 Handle을 Release하고, Resources인 경우 UnloadAsset을 진행합니다.
        /// </summary>
        public void ReleaseAsset()
        {
            if (HasAddressableHandle && AddressableHandle.IsValid())
            {
                Addressables.Release(AddressableHandle);
                HasAddressableHandle = false;
            }
            else if (Source == AssetSource.Resources && Asset != null)
            {
                // GameObject 및 Component 등 Prefab 자원은 Resources.UnloadAsset 사용 불가이므로 비-GameObject만 Unload
                if (!(Asset is GameObject) && !(Asset is Component))
                {
                    Resources.UnloadAsset(Asset);
                }
            }

            Asset = null;
        }
    }
}
