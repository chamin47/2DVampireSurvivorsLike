using System;
using UnityEngine;

namespace Framework.Audio
{
    /// <summary>
    /// 단일 오디오 클립 식별 정보 및 기본 음향 설정을 정의하는 직렬화 클래스입니다.
    /// AudioClip 직접 참조 및 Addressable/Resource 주소를 지원합니다.
    /// </summary>
    [Serializable]
    public class AudioDefinition
    {
        [Tooltip("오디오 식별 Enum 타입")]
        public AudioType type;

        [Tooltip("직접 참조용 AudioClip (우선 순위 high)")]
        public AudioClip clip;

        [Tooltip("Addressables 주소 또는 Resources 경로 (clip이 없을 경우 지정)")]
        public string address;

        [Range(0f, 1f)]
        [Tooltip("기본 재생 볼륨 (0.0 ~ 1.0)")]
        public float defaultVolume = 1.0f;

        [Range(0.1f, 3f)]
        [Tooltip("기본 재생 피치 (0.1 ~ 3.0)")]
        public float defaultPitch = 1.0f;

        public AudioDefinition()
        {
        }

        public AudioDefinition(AudioType type, AudioClip clip, float defaultVolume = 1.0f, float defaultPitch = 1.0f)
        {
            this.type = type;
            this.clip = clip;
            this.defaultVolume = defaultVolume;
            this.defaultPitch = defaultPitch;
        }

        public AudioDefinition(AudioType type, string address, float defaultVolume = 1.0f, float defaultPitch = 1.0f)
        {
            this.type = type;
            this.address = address;
            this.defaultVolume = defaultVolume;
            this.defaultPitch = defaultPitch;
        }
    }
}
