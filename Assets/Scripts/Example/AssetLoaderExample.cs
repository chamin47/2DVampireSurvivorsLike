using System.Threading.Tasks;
using UnityEngine;
using Framework.Asset;

namespace Example
{
    /// <summary>
    /// AssetLoader 시스템 사용법을 안내하는 샘플 컴포넌트입니다.
    /// </summary>
    public class AssetLoaderExample : MonoBehaviour
    {
        [Header("Asset Keys")]
        [SerializeField] private string characterKey = "Character/CaoCao";
        [SerializeField] private string effectKey = "Effect/Hit";
        [SerializeField] private string atlasKey = "CharacterAtlas";
        [SerializeField] private string spriteName = "CaoCao";

        private async void Start()
        {
            Debug.Log("=== AssetLoader Example Started ===");

            // 1. Asset Definition 등록 (선택 사항)
            AssetLoader.RegisterDefinition(new AssetDefinition(characterKey, characterKey, AssetSource.Resources));

            // 2. 동기 Prefab Load & Instantiate
            GameObject prefab = AssetLoader.Load<GameObject>(characterKey);
            if (prefab != null)
            {
                Debug.Log($"[Example] Loaded prefab: {prefab.name}");
            }

            GameObject characterObj = AssetLoader.Instantiate(characterKey, transform);
            if (characterObj != null)
            {
                Debug.Log($"[Example] Instantiated character: {characterObj.name}");
            }

            // 3. 비동기 LoadAsync
            GameObject asyncPrefab = await AssetLoader.LoadAsync<GameObject>(characterKey);
            if (asyncPrefab != null)
            {
                Debug.Log($"[Example] Async loaded prefab: {asyncPrefab.name}");
            }

            // 4. Sprite Atlas 및 Sprite 조회
            Sprite sprite = AssetLoader.LoadSprite(atlasKey, spriteName);
            if (sprite != null)
            {
                Debug.Log($"[Example] Loaded sprite '{spriteName}' from atlas '{atlasKey}'");
            }

            // 5. Object Pool (Spawn & Despawn)
            GameObject pooledEffect = AssetLoader.Spawn(effectKey, transform);
            if (pooledEffect != null)
            {
                Debug.Log($"[Example] Spawned pooled effect: {pooledEffect.name}");
                
                // 사용 후 Pool로 반환
                AssetLoader.Despawn(pooledEffect);
                Debug.Log($"[Example] Despawned effect back to pool.");
            }

            // 6. Release (생성된 인스턴스 파괴/반환)
            if (characterObj != null)
            {
                AssetLoader.Release(characterObj);
                Debug.Log("[Example] Released character object.");
            }

            // 7. Asset Unload (캐시된 Asset 해제)
            AssetLoader.Unload(characterKey);

            Debug.Log("=== AssetLoader Example Completed ===");
        }

        private void OnDestroy()
        {
            // 씬 파괴 시 Asset 전체 정리 (필요 시)
            // AssetLoader.ReleaseAll();
        }
    }
}
