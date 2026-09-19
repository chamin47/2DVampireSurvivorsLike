namespace Framework.Effect
{
    /// <summary>
    /// 게임 내에서 사용할 이펙트 리소스 식별 키 Enum입니다.
    /// </summary>
    public enum EffectType
    {
        None = 0,

        // Common Combat Effects
        Attack,
        Hit,
        CriticalHit,
        Heal,
        LevelUp,

        // Elemental / Special Effects
        Fire,
        Explosion,

        // Status Effects
        Buff,
        Debuff
    }

    /// <summary>
    /// 이펙트 리소스의 대표 재생 형태 타입입니다.
    /// </summary>
    public enum EffectPlayType
    {
        Particle,   // ParticleSystem 기반 이펙트
        Animation,  // Animation 또는 Animator 기반 이펙트
        Prefab      // 복합 또는 일반 Prefab (VFX Graph, Timeline 등 포함)
    }
}
