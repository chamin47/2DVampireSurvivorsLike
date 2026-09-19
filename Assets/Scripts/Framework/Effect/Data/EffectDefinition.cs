using System;
using UnityEngine;
using Framework.Audio;

namespace Framework.Effect
{
    /// <summary>
    /// EffectType과 실제 이펙트 리소스 및 재생 설정을 연결하는 이펙트 데이터 클래스입니다.
    /// </summary>
    [Serializable]
    public class EffectDefinition
    {
        [Tooltip("이펙트 식별 Enum 키")]
        public EffectType type = EffectType.None;

        [Tooltip("AssetLoader를 통해 로드할 주소/리소스 키 (미지정 시 'Effects/Effect_{Type}' 자동 사용)")]
        public string addressableKey;

        [Tooltip("이펙트의 주요 재생 방식")]
        public EffectPlayType playType = EffectPlayType.Particle;

        [Tooltip("이펙트 재생 지속 시간 (0 이하 지정 시 ParticleSystem의 최대 지속시간으로 자동 계산)")]
        public float duration = 0f;

        [Tooltip("지속시간 경과 후 자동으로 Object Pool로 반환할지 여부")]
        public bool autoRelease = true;

        [Tooltip("생성 위치 오프셋 (부모 또는 지정 좌표 기준)")]
        public Vector3 positionOffset = Vector3.zero;

        [Tooltip("생성 회전 오프셋 (Euler angles)")]
        public Vector3 rotationOffset = Vector3.zero;

        [Tooltip("이펙트 스케일 (기본값: (1, 1, 1))")]
        public Vector3 scale = Vector3.one;

        [Tooltip("부모 Transform 전달 시 자식 객체로 위치를 추적(부모 할당)할지 여부")]
        public bool attachToParent = true;

        [Tooltip("이펙트 재생 시 함께 출력할 SFX 사운드 (None이면 미출력)")]
        public Framework.Audio.AudioType sfxType = Framework.Audio.AudioType.None;

        public EffectDefinition()
        {
            type = EffectType.None;
            addressableKey = string.Empty;
            playType = EffectPlayType.Particle;
            duration = 0f;
            autoRelease = true;
            positionOffset = Vector3.zero;
            rotationOffset = Vector3.zero;
            scale = Vector3.one;
            attachToParent = true;
            sfxType = Framework.Audio.AudioType.None;
        }

        public EffectDefinition(EffectType type, string addressableKey, EffectPlayType playType = EffectPlayType.Particle, float duration = 0f)
        {
            this.type = type;
            this.addressableKey = addressableKey;
            this.playType = playType;
            this.duration = duration;
            this.autoRelease = true;
            this.positionOffset = Vector3.zero;
            this.rotationOffset = Vector3.zero;
            this.scale = Vector3.one;
            this.attachToParent = true;
            this.sfxType = Framework.Audio.AudioType.None;
        }
    }
}
