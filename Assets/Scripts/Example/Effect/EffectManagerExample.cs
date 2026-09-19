using System.Threading.Tasks;
using UnityEngine;
using Framework.Effect;
using Framework.Event;

namespace Example
{
    /// <summary>
    /// EffectManager 시스템의 주요 기능과 사용법을 안내하는 샘플 컴포넌트입니다.
    /// </summary>
    public class EffectManagerExample : MonoBehaviour
    {
        [Header("Target Parent Transform")]
        [SerializeField] private Transform targetTransform;

        private void OnEnable()
        {
            // EventBus 이벤트 구독
            EventBus.Subscribe<OnEffectPlayedEvent>(OnEffectPlayed);
            EventBus.Subscribe<OnEffectStoppedEvent>(OnEffectStopped);
        }

        private void OnDisable()
        {
            // EventBus 이벤트 구독 해제
            EventBus.Unsubscribe<OnEffectPlayedEvent>(OnEffectPlayed);
            EventBus.Unsubscribe<OnEffectStoppedEvent>(OnEffectStopped);
        }

        private async void Start()
        {
            Debug.Log("=== EffectManager Example Started ===");

            if (targetTransform == null)
            {
                targetTransform = transform;
            }

            // 1. 커스텀 EffectDefinition 등록 (필요 시)
            EffectManager.RegisterDefinition(new EffectDefinition
            {
                type = EffectType.Attack,
                addressableKey = "Effects/Effect_Attack",
                playType = EffectPlayType.Particle,
                duration = 1.5f,
                autoRelease = true,
                positionOffset = new Vector3(0, 1.0f, 0)
            });

            // 2. 기본 동기 이펙트 재생 (Target Transform에 부착)
            Debug.Log("[Example] Playing Attack effect synchronously...");
            EffectInstance attackInstance = EffectManager.Play(EffectType.Attack, targetTransform);

            if (attackInstance != null)
            {
                Debug.Log($"[Example] Played Attack Effect. IsPlaying: {attackInstance.IsPlaying}");
            }

            // 3. 월드 좌표를 지정하여 동기 이펙트 재생
            Vector3 spawnPos = new Vector3(2.0f, 0.5f, 0.0f);
            Debug.Log($"[Example] Playing Hit effect at world position {spawnPos}...");
            EffectInstance hitInstance = EffectManager.Play(EffectType.Hit, spawnPos);

            // 4. 비동기 이펙트 재생 (PlayAsync)
            Debug.Log("[Example] Playing CriticalHit effect asynchronously...");
            EffectInstance criticalInstance = await EffectManager.PlayAsync(EffectType.CriticalHit, targetTransform);

            if (criticalInstance != null)
            {
                Debug.Log($"[Example] Played CriticalHit Effect via PlayAsync. IsPlaying: {criticalInstance.IsPlaying}");
            }

            // 5. EffectInstance 제어 (1초 후 수동 정지 예시)
            await Task.Delay(1000);
            if (attackInstance != null && attackInstance.IsPlaying)
            {
                Debug.Log("[Example] Manually stopping Attack Effect via EffectInstance handle...");
                attackInstance.Stop();
            }

            Debug.Log("=== EffectManager Example Initialization Completed ===");
        }

        private void OnEffectPlayed(OnEffectPlayedEvent evt)
        {
            Debug.Log($"[EventBus] Effect Played Event -> Type: {evt.Type}, GameObject: {evt.EffectGameObject.name}");
        }

        private void OnEffectStopped(OnEffectStoppedEvent evt)
        {
            Debug.Log($"[EventBus] Effect Stopped Event -> Type: {evt.Type}, GameObject: {evt.EffectGameObject.name}");
        }
    }
}
