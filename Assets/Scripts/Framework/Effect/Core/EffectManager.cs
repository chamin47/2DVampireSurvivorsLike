using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Framework.Asset;
using Framework.Audio;
using Framework.Event;

namespace Framework.Effect
{
    /// <summary>
    /// Enum 키 기반으로 이펙트 프리팹 로딩, Object Pooling(AssetLoader 연동),
    /// 재생 및 생명주기를 총괄 관리하는 중앙 EffectManager Singleton입니다.
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        private static EffectManager instance;

        /// <summary>
        /// EffectManager 전역 Singleton 인스턴스입니다.
        /// </summary>
        public static EffectManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<EffectManager>();
                    if (instance == null)
                    {
                        var go = new GameObject("[EffectManager]");
                        instance = go.AddComponent<EffectManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("Data Configuration")]
        [SerializeField]
        private EffectDataSO effectDataSO;

        private Transform effectRoot;
        private readonly Dictionary<EffectType, EffectDefinition> definitionMap = new Dictionary<EffectType, EffectDefinition>();
        private readonly List<EffectInstance> activeInstances = new List<EffectInstance>();

        #region Unity Lifecycle

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeHierarchy();

            if (effectDataSO == null)
            {
                effectDataSO = Resources.Load<EffectDataSO>("Effect/EffectData");
            }

            if (effectDataSO != null)
            {
                effectDataSO.Initialize();
                foreach (var def in effectDataSO.Definitions)
                {
                    RegisterDefinition(def);
                }
            }
        }

        private void InitializeHierarchy()
        {
            effectRoot = transform.Find("[EffectRoot]");
            if (effectRoot == null)
            {
                var go = new GameObject("[EffectRoot]");
                go.transform.SetParent(transform, false);
                effectRoot = go.transform;
            }
        }

        #endregion

        #region Configuration & Definition Management

        /// <summary>
        /// EffectDataSO 데이터 자산을 설정합니다.
        /// </summary>
        public static void SetEffectData(EffectDataSO data)
        {
            Instance.InternalSetEffectData(data);
        }

        private void InternalSetEffectData(EffectDataSO data)
        {
            effectDataSO = data;
            if (effectDataSO != null)
            {
                effectDataSO.Initialize();
                foreach (var def in effectDataSO.Definitions)
                {
                    RegisterDefinition(def);
                }
            }
        }

        /// <summary>
        /// 특정 EffectDefinition 정의를 동적으로 등록합니다.
        /// </summary>
        public static void RegisterDefinition(EffectDefinition definition)
        {
            if (definition == null || definition.type == EffectType.None) return;
            Instance.definitionMap[definition.type] = definition;
        }

        /// <summary>
        /// EffectType에 대한 EffectDefinition 정보를 가져옵니다 (미등록 시 폴백 생성).
        /// </summary>
        public static EffectDefinition GetDefinition(EffectType type)
        {
            return Instance.InternalGetDefinition(type);
        }

        private EffectDefinition InternalGetDefinition(EffectType type)
        {
            if (definitionMap.TryGetValue(type, out var def))
            {
                return def;
            }

            // 폴백 Definition 생성: Key = "Effects/Effect_{type}"
            var fallbackDef = new EffectDefinition(type, $"Effects/Effect_{type}");
            definitionMap[type] = fallbackDef;
            return fallbackDef;
        }

        #endregion

        #region Synchronous Play API

        /// <summary>
        /// EffectType 키를 통해 부모 Transform에 이펙트를 재생합니다.
        /// </summary>
        public static EffectInstance Play(EffectType type, Transform parent = null, Vector3 positionOffset = default, Quaternion rotationOffset = default)
        {
            return Instance.InternalPlay(type, parent, positionOffset, rotationOffset);
        }

        /// <summary>
        /// EffectType 키를 통해 특정 월드 위치 및 회전에서 이펙트를 재생합니다.
        /// </summary>
        public static EffectInstance Play(EffectType type, Vector3 worldPosition, Quaternion worldRotation = default, Transform parent = null)
        {
            return Instance.InternalPlayAtPosition(type, worldPosition, worldRotation, parent);
        }

        private EffectInstance InternalPlay(EffectType type, Transform parent, Vector3 extraPosOffset, Quaternion extraRotOffset)
        {
            if (type == EffectType.None) return null;

            var def = InternalGetDefinition(type);
            string assetKey = string.IsNullOrEmpty(def.addressableKey) ? $"Effects/Effect_{type}" : def.addressableKey;

            Transform targetParent = (parent != null && def.attachToParent) ? parent : effectRoot;
            GameObject effectGo = AssetLoader.Spawn(assetKey, targetParent);

            if (effectGo == null)
            {
                Debug.LogError($"[EffectManager] Failed to spawn effect prefab for EffectType '{type}' with Key '{assetKey}'.");
                return null;
            }

            // Transform 위치/회전/스케일 설정
            Vector3 finalPosOffset = def.positionOffset + extraPosOffset;
            Quaternion finalRotOffset = Quaternion.Euler(def.rotationOffset) * (extraRotOffset == default ? Quaternion.identity : extraRotOffset);

            if (parent != null && def.attachToParent)
            {
                effectGo.transform.localPosition = finalPosOffset;
                effectGo.transform.localRotation = finalRotOffset;
            }
            else if (parent != null)
            {
                effectGo.transform.position = parent.position + (parent.rotation * finalPosOffset);
                effectGo.transform.rotation = parent.rotation * finalRotOffset;
            }
            else
            {
                effectGo.transform.position = finalPosOffset;
                effectGo.transform.rotation = finalRotOffset;
            }

            effectGo.transform.localScale = def.scale;

            // EffectPlayer 조율
            if (!effectGo.TryGetComponent<EffectPlayer>(out var player))
            {
                player = effectGo.AddComponent<EffectPlayer>();
            }

            EffectInstance instanceHandle = new EffectInstance(type, effectGo, player);
            player.Init(def, instanceHandle, OnEffectCompleted);

            activeInstances.Add(instanceHandle);

            // AudioPlayer SFX 연동 (설정되어 있는 경우)
            if (def.sfxType != Framework.Audio.AudioType.None)
            {
                AudioPlayer.PlaySfx(def.sfxType);
            }

            // Play 실행
            player.Play();

            // EventBus 이벤트 발행
            EventBus.Publish(new OnEffectPlayedEvent(type, effectGo, parent, instanceHandle));

            return instanceHandle;
        }

        private EffectInstance InternalPlayAtPosition(EffectType type, Vector3 worldPosition, Quaternion worldRotation, Transform parent)
        {
            if (type == EffectType.None) return null;

            if (worldRotation == default) worldRotation = Quaternion.identity;

            EffectInstance instance = InternalPlay(type, parent, Vector3.zero, Quaternion.identity);
            if (instance != null && instance.GameObject != null)
            {
                instance.GameObject.transform.position = worldPosition + instance.Player.Definition.positionOffset;
                instance.GameObject.transform.rotation = worldRotation * Quaternion.Euler(instance.Player.Definition.rotationOffset);
            }

            return instance;
        }

        #endregion

        #region Asynchronous Play API

        /// <summary>
        /// EffectType 키를 통해 비동기로 프리팹을 로드하고 재생합니다.
        /// </summary>
        public static async Task<EffectInstance> PlayAsync(EffectType type, Transform parent = null, Vector3 positionOffset = default, Quaternion rotationOffset = default)
        {
            return await Instance.InternalPlayAsync(type, parent, positionOffset, rotationOffset);
        }

        /// <summary>
        /// EffectType 키를 통해 특정 월드 위치에서 비동기로 프리팹을 로드하고 재생합니다.
        /// </summary>
        public static async Task<EffectInstance> PlayAsync(EffectType type, Vector3 worldPosition, Quaternion worldRotation = default, Transform parent = null)
        {
            var inst = await Instance.InternalPlayAsync(type, parent, Vector3.zero, Quaternion.identity);
            if (inst != null && inst.GameObject != null)
            {
                if (worldRotation == default) worldRotation = Quaternion.identity;
                inst.GameObject.transform.position = worldPosition + inst.Player.Definition.positionOffset;
                inst.GameObject.transform.rotation = worldRotation * Quaternion.Euler(inst.Player.Definition.rotationOffset);
            }
            return inst;
        }

        private async Task<EffectInstance> InternalPlayAsync(EffectType type, Transform parent, Vector3 extraPosOffset, Quaternion extraRotOffset)
        {
            if (type == EffectType.None) return null;

            var def = InternalGetDefinition(type);
            string assetKey = string.IsNullOrEmpty(def.addressableKey) ? $"Effects/Effect_{type}" : def.addressableKey;

            // AssetLoader를 이용해 비동기 Preload 확인
            await AssetLoader.LoadAsync<GameObject>(assetKey);

            return InternalPlay(type, parent, extraPosOffset, extraRotOffset);
        }

        /// <summary>
        /// 특정 이펙트 Asset을 미리 로드(Preload)합니다.
        /// </summary>
        public static async Task PreloadAsync(EffectType type)
        {
            if (type == EffectType.None) return;
            var def = Instance.InternalGetDefinition(type);
            string assetKey = string.IsNullOrEmpty(def.addressableKey) ? $"Effects/Effect_{type}" : def.addressableKey;
            await AssetLoader.LoadAsync<GameObject>(assetKey);
        }

        #endregion

        #region Stop & Lifetime Management

        /// <summary>
        /// 지정한 EffectInstance의 재생을 정지하고 풀로 반환합니다.
        /// </summary>
        public static void Stop(EffectInstance instanceHandle)
        {
            if (instanceHandle != null)
            {
                instanceHandle.Stop();
            }
        }

        /// <summary>
        /// 현재 재생 중인 모든 이펙트를 정지하고 풀로 반환합니다.
        /// </summary>
        public static void StopAll()
        {
            Instance.InternalStopAll();
        }

        private void InternalStopAll()
        {
            var snapshot = activeInstances.ToArray();
            foreach (var inst in snapshot)
            {
                if (inst != null && inst.IsValid)
                {
                    inst.Stop();
                }
            }
            activeInstances.Clear();
        }

        private void OnEffectCompleted(EffectPlayer player)
        {
            if (player == null) return;

            EffectInstance inst = player.InstanceHandle;
            if (inst != null)
            {
                activeInstances.Remove(inst);
                EventBus.Publish(new OnEffectStoppedEvent(player.Definition != null ? player.Definition.type : EffectType.None, player.gameObject, inst));
            }

            // AssetLoader를 통해 Pooled GameObject 반환
            AssetLoader.Despawn(player.gameObject);
        }

        #endregion
    }
}
