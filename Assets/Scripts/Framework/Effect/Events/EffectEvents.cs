using UnityEngine;

namespace Framework.Effect
{
    /// <summary>
    /// 이펙트 재생 시 발행되는 이벤트 구조체입니다.
    /// EventBus.Publish(new OnEffectPlayedEvent(...)) 형태로 사용됩니다.
    /// </summary>
    public readonly struct OnEffectPlayedEvent
    {
        public readonly EffectType Type;
        public readonly GameObject EffectGameObject;
        public readonly Transform Parent;
        public readonly EffectInstance Instance;

        public OnEffectPlayedEvent(EffectType type, GameObject effectGameObject, Transform parent, EffectInstance instance)
        {
            Type = type;
            EffectGameObject = effectGameObject;
            Parent = parent;
            Instance = instance;
        }
    }

    /// <summary>
    /// 이펙트 재생 종료 / 풀 반환 시 발행되는 이벤트 구조체입니다.
    /// </summary>
    public readonly struct OnEffectStoppedEvent
    {
        public readonly EffectType Type;
        public readonly GameObject EffectGameObject;
        public readonly EffectInstance Instance;

        public OnEffectStoppedEvent(EffectType type, GameObject effectGameObject, EffectInstance instance)
        {
            Type = type;
            EffectGameObject = effectGameObject;
            Instance = instance;
        }
    }
}
