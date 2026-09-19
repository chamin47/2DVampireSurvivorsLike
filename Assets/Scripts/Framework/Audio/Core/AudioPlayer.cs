using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Audio
{
    /// <summary>
    /// SFX 재생용 AudioSource 및 재생 상태 정보 래퍼입니다.
    /// </summary>
    internal class SfxAudioSourceEntry
    {
        public AudioSource source;
        public AudioType currentType;
        public float baseVolume;
    }

    /// <summary>
    /// 게임 내 BGM 및 SFX 재생, Fade In/Out, Volume/Mute 제어, AudioSource Pooling을 총괄 관리하는 Singleton AudioPlayer입니다.
    /// </summary>
    public class AudioPlayer : MonoBehaviour
    {
        private static AudioPlayer instance;

        /// <summary>
        /// AudioPlayer 전역 Singleton 인스턴스입니다.
        /// </summary>
        public static AudioPlayer Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<AudioPlayer>();
                    if (instance == null)
                    {
                        var go = new GameObject("[AudioPlayer]");
                        instance = go.AddComponent<AudioPlayer>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("Audio Data")]
        [SerializeField]
        private AudioDataSO audioDataSO;

        [Header("Initial Settings")]
        [SerializeField] private int initialSfxPoolSize = 6;
        [SerializeField, Range(0f, 1f)] private float masterVolume = 1.0f;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 1.0f;
        [SerializeField, Range(0f, 1f)] private float sfxVolume = 1.0f;

        [SerializeField] private bool isMasterMuted = false;
        [SerializeField] private bool isBgmMuted = false;
        [SerializeField] private bool isSfxMuted = false;

        // Hierarchical Transforms & Components
        private Transform bgmRoot;
        private Transform sfxRoot;
        private AudioSource bgmSource;
        private readonly List<SfxAudioSourceEntry> sfxPool = new List<SfxAudioSourceEntry>();

        // State Tracking
        private AudioType currentBgmType = AudioType.None;
        private float currentBgmBaseVolume = 1.0f;
        private Coroutine bgmFadeCoroutine;

        // Custom External Clip Loader Callback (e.g., for Addressables async loader)
        public static Func<string, AudioClip> CustomClipLoader;

        #region Properties

        /// <summary>
        /// 현재 재생 중인 BGM의 AudioType입니다.
        /// </summary>
        public AudioType CurrentBgmType => currentBgmType;

        /// <summary>
        /// BGM이 현재 재생 중인지 여부입니다.
        /// </summary>
        public bool IsBgmPlaying => bgmSource != null && bgmSource.isPlaying;

        /// <summary>
        /// Master Volume 프로퍼티입니다. (0.0 ~ 1.0)
        /// </summary>
        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Mathf.Clamp01(value);
                ApplyVolumeSettings();
            }
        }

        /// <summary>
        /// BGM Volume 프로퍼티입니다. (0.0 ~ 1.0)
        /// </summary>
        public float BgmVolume
        {
            get => bgmVolume;
            set
            {
                bgmVolume = Mathf.Clamp01(value);
                ApplyVolumeSettings();
            }
        }

        /// <summary>
        /// SFX Volume 프로퍼티입니다. (0.0 ~ 1.0)
        /// </summary>
        public float SfxVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                ApplyVolumeSettings();
            }
        }

        /// <summary>
        /// Master Mute 상태 여부입니다.
        /// </summary>
        public bool IsMasterMuted
        {
            get => isMasterMuted;
            set
            {
                isMasterMuted = value;
                ApplyVolumeSettings();
            }
        }

        /// <summary>
        /// BGM Mute 상태 여부입니다.
        /// </summary>
        public bool IsBgmMuted
        {
            get => isBgmMuted;
            set
            {
                isBgmMuted = value;
                ApplyVolumeSettings();
            }
        }

        /// <summary>
        /// SFX Mute 상태 여부입니다.
        /// </summary>
        public bool IsSfxMuted
        {
            get => isSfxMuted;
            set
            {
                isSfxMuted = value;
                ApplyVolumeSettings();
            }
        }

        #endregion

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

            if (audioDataSO == null)
            {
                audioDataSO = Resources.Load<AudioDataSO>("Audio/AudioData");
            }

            if (audioDataSO != null)
            {
                audioDataSO.Initialize();
            }
        }

        private void Update()
        {
            // 주기적으로 마친 SFX AudioSource 상태 정리 (Pool 재사용용)
            for (int i = 0; i < sfxPool.Count; i++)
            {
                var entry = sfxPool[i];
                if (entry.source != null && !entry.source.isPlaying && entry.currentType != AudioType.None)
                {
                    entry.currentType = AudioType.None;
                }
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// AudioPlayer GameObject 하위에 Bgm, Sfx 트랜스폼 및 AudioSource를 구성합니다.
        /// </summary>
        private void InitializeHierarchy()
        {
            // 1. Bgm Root & AudioSource Setup
            bgmRoot = transform.Find("Bgm");
            if (bgmRoot == null)
            {
                var bgmGo = new GameObject("Bgm");
                bgmGo.transform.SetParent(transform, false);
                bgmRoot = bgmGo.transform;
            }

            bgmSource = bgmRoot.GetComponent<AudioSource>();
            if (bgmSource == null)
            {
                bgmSource = bgmRoot.gameObject.AddComponent<AudioSource>();
            }
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;

            // 2. Sfx Root & Pool Setup
            sfxRoot = transform.Find("Sfx");
            if (sfxRoot == null)
            {
                var sfxGo = new GameObject("Sfx");
                sfxGo.transform.SetParent(transform, false);
                sfxRoot = sfxGo.transform;
            }

            sfxPool.Clear();
            var existingSources = sfxRoot.GetComponentsInChildren<AudioSource>();
            foreach (var src in existingSources)
            {
                sfxPool.Add(new SfxAudioSourceEntry
                {
                    source = src,
                    currentType = AudioType.None,
                    baseVolume = 1.0f
                });
            }

            // initialSfxPoolSize만큼 미리 생성 (Pre-warm)
            while (sfxPool.Count < initialSfxPoolSize)
            {
                CreateNewSfxAudioSource();
            }
        }

        private SfxAudioSourceEntry CreateNewSfxAudioSource()
        {
            var sfxGo = new GameObject($"SfxAudioSource_{sfxPool.Count + 1}");
            sfxGo.transform.SetParent(sfxRoot, false);
            var src = sfxGo.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;

            var entry = new SfxAudioSourceEntry
            {
                source = src,
                currentType = AudioType.None,
                baseVolume = 1.0f
            };
            sfxPool.Add(entry);
            return entry;
        }

        /// <summary>
        /// AudioDataSO 자산을 동적으로 설정합니다.
        /// </summary>
        public void SetAudioData(AudioDataSO data)
        {
            audioDataSO = data;
            if (audioDataSO != null)
            {
                audioDataSO.Initialize();
            }
        }

        #endregion

        #region BGM Play / Stop API

        /// <summary>
        /// AudioType을 지정하여 BGM을 재생합니다.
        /// </summary>
        /// <param name="type">재생할 BGM AudioType</param>
        /// <param name="loop">반복 재생 여부 (기본값: true)</param>
        /// <param name="fadeDuration">BGM 교체/시작 Fade 시간 (기본값: 1.0초)</param>
        /// <param name="volume">개별 BGM 볼륨 배율 (기본값: 1.0)</param>
        public static void PlayBgm(AudioType type, bool loop = true, float fadeDuration = 1.0f, float volume = 1.0f)
        {
            Instance.InternalPlayBgm(type, loop, fadeDuration, volume);
        }

        /// <summary>
        /// AudioClip을 직접 전달하여 BGM을 재생합니다.
        /// </summary>
        public static void PlayBgm(AudioClip clip, bool loop = true, float fadeDuration = 1.0f, float volume = 1.0f)
        {
            Instance.InternalPlayBgm(clip, AudioType.None, loop, fadeDuration, volume);
        }

        /// <summary>
        /// 현재 재생 중인 BGM을 정지합니다.
        /// </summary>
        /// <param name="fadeDuration">Fade Out 시간 (기본값: 1.0초, 0인 경우 즉시 정지)</param>
        public static void StopBgm(float fadeDuration = 1.0f)
        {
            Instance.InternalStopBgm(fadeDuration);
        }

        private void InternalPlayBgm(AudioType type, bool loop, float fadeDuration, float volume)
        {
            if (type == AudioType.None)
            {
                InternalStopBgm(fadeDuration);
                return;
            }

            // 동일한 BGM이 이미 재생 중이고 정지 상태가 아니면 재생 건너뜀
            if (currentBgmType == type && bgmSource != null && bgmSource.isPlaying && bgmFadeCoroutine == null)
            {
                return;
            }

            AudioClip clip = ResolveClip(type, out var def);
            float targetVolumeScale = volume * (def != null ? def.defaultVolume : 1.0f);
            float pitch = def != null ? def.defaultPitch : 1.0f;

            if (clip == null)
            {
                Debug.LogWarning($"[AudioPlayer] Failed to load AudioClip for AudioType: {type}");
                return;
            }

            InternalPlayBgm(clip, type, loop, fadeDuration, targetVolumeScale, pitch);
        }

        private void InternalPlayBgm(AudioClip clip, AudioType type, bool loop, float fadeDuration, float volumeScale, float pitch = 1.0f)
        {
            if (clip == null) return;

            if (bgmFadeCoroutine != null)
            {
                StopCoroutine(bgmFadeCoroutine);
                bgmFadeCoroutine = null;
            }

            currentBgmType = type;
            currentBgmBaseVolume = volumeScale;
            bgmSource.pitch = pitch;

            if (fadeDuration <= 0f || !bgmSource.isPlaying)
            {
                // 즉시 전환
                bgmSource.clip = clip;
                bgmSource.loop = loop;
                bgmSource.volume = GetCalculatedBgmVolume(currentBgmBaseVolume);
                bgmSource.Play();
            }
            else
            {
                // Fade Out -> Change Clip -> Fade In Coroutine 실행
                bgmFadeCoroutine = StartCoroutine(CoFadeBgm(clip, loop, fadeDuration, currentBgmBaseVolume));
            }
        }

        private void InternalStopBgm(float fadeDuration)
        {
            if (bgmSource == null || !bgmSource.isPlaying)
            {
                currentBgmType = AudioType.None;
                return;
            }

            if (bgmFadeCoroutine != null)
            {
                StopCoroutine(bgmFadeCoroutine);
                bgmFadeCoroutine = null;
            }

            if (fadeDuration <= 0f)
            {
                bgmSource.Stop();
                bgmSource.clip = null;
                currentBgmType = AudioType.None;
            }
            else
            {
                bgmFadeCoroutine = StartCoroutine(CoFadeOutAndStopBgm(fadeDuration));
            }
        }

        private IEnumerator CoFadeBgm(AudioClip newClip, bool loop, float fadeDuration, float targetVolumeScale)
        {
            float halfDuration = fadeDuration * 0.5f;
            float startVolume = bgmSource.volume;

            // 1. Fade Out
            float elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float factor = 1f - (elapsed / halfDuration);
                bgmSource.volume = startVolume * Mathf.Clamp01(factor);
                yield return null;
            }

            bgmSource.Stop();

            // 2. Change Clip & Play
            bgmSource.clip = newClip;
            bgmSource.loop = loop;
            bgmSource.volume = 0f;
            bgmSource.Play();

            // 3. Fade In
            elapsed = 0f;
            float maxCalculatedVol = GetCalculatedBgmVolume(targetVolumeScale);
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float factor = elapsed / halfDuration;
                bgmSource.volume = maxCalculatedVol * Mathf.Clamp01(factor);
                yield return null;
            }

            bgmSource.volume = GetCalculatedBgmVolume(targetVolumeScale);
            bgmFadeCoroutine = null;
        }

        private IEnumerator CoFadeOutAndStopBgm(float fadeDuration)
        {
            float startVolume = bgmSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float factor = 1f - (elapsed / fadeDuration);
                bgmSource.volume = startVolume * Mathf.Clamp01(factor);
                yield return null;
            }

            bgmSource.Stop();
            bgmSource.clip = null;
            currentBgmType = AudioType.None;
            bgmFadeCoroutine = null;
        }

        #endregion

        #region SFX Play / Stop API

        /// <summary>
        /// AudioType을 지정하여 SFX를 재생합니다. (다중 동시 재생 가능)
        /// </summary>
        /// <param name="type">재생할 SFX AudioType</param>
        /// <param name="volume">개별 볼륨 배율 (기본값: 1.0)</param>
        /// <param name="pitch">개별 피치 배율 (기본값: 1.0)</param>
        public static void PlaySfx(AudioType type, float volume = 1.0f, float pitch = 1.0f)
        {
            Instance.InternalPlaySfx(type, volume, pitch);
        }

        /// <summary>
        /// AudioClip을 직접 전달하여 SFX를 재생합니다.
        /// </summary>
        public static void PlaySfx(AudioClip clip, float volume = 1.0f, float pitch = 1.0f)
        {
            Instance.InternalPlaySfx(clip, AudioType.None, volume, pitch);
        }

        /// <summary>
        /// SFX 재생을 정지합니다. type을 지정하면 해당 타입의 SFX만, AudioType.None이면 모든 SFX를 정지합니다.
        /// </summary>
        /// <param name="type">정지할 SFX AudioType (기본값: None = 전체 정지)</param>
        public static void StopSfx(AudioType type = AudioType.None)
        {
            Instance.InternalStopSfx(type);
        }

        private void InternalPlaySfx(AudioType type, float volume, float pitch)
        {
            if (type == AudioType.None) return;

            AudioClip clip = ResolveClip(type, out var def);
            if (clip == null)
            {
                Debug.LogWarning($"[AudioPlayer] Failed to load AudioClip for SFX AudioType: {type}");
                return;
            }

            float finalVolumeScale = volume * (def != null ? def.defaultVolume : 1.0f);
            float finalPitch = pitch * (def != null ? def.defaultPitch : 1.0f);

            InternalPlaySfx(clip, type, finalVolumeScale, finalPitch);
        }

        private void InternalPlaySfx(AudioClip clip, AudioType type, float volumeScale, float pitch)
        {
            if (clip == null) return;

            SfxAudioSourceEntry availableEntry = GetAvailableSfxSource();
            availableEntry.currentType = type;
            availableEntry.baseVolume = volumeScale;
            availableEntry.source.clip = clip;
            availableEntry.source.pitch = pitch;
            availableEntry.source.volume = GetCalculatedSfxVolume(volumeScale);
            availableEntry.source.Play();
        }

        private SfxAudioSourceEntry GetAvailableSfxSource()
        {
            // 1. 재생 중이 아닌 비어있는 Pool 찾기
            foreach (var entry in sfxPool)
            {
                if (!entry.source.isPlaying)
                {
                    return entry;
                }
            }

            // 2. 사용 가능한 Source가 없으면 Pool 동적 확장
            return CreateNewSfxAudioSource();
        }

        private void InternalStopSfx(AudioType type)
        {
            foreach (var entry in sfxPool)
            {
                if (entry.source == null) continue;

                if (type == AudioType.None || entry.currentType == type)
                {
                    entry.source.Stop();
                    entry.currentType = AudioType.None;
                }
            }
        }

        #endregion

        #region Mute & Volume Helpers

        /// <summary>
        /// Master Volume 설정 (0.0 ~ 1.0)
        /// </summary>
        public static void SetMasterVolume(float volume) => Instance.MasterVolume = volume;

        /// <summary>
        /// BGM Volume 설정 (0.0 ~ 1.0)
        /// </summary>
        public static void SetBgmVolume(float volume) => Instance.BgmVolume = volume;

        /// <summary>
        /// SFX Volume 설정 (0.0 ~ 1.0)
        /// </summary>
        public static void SetSfxVolume(float volume) => Instance.SfxVolume = volume;

        /// <summary>
        /// Master Mute 설정
        /// </summary>
        public static void MuteMaster(bool mute) => Instance.IsMasterMuted = mute;

        /// <summary>
        /// BGM Mute 설정
        /// </summary>
        public static void MuteBgm(bool mute) => Instance.IsBgmMuted = mute;

        /// <summary>
        /// SFX Mute 설정
        /// </summary>
        public static void MuteSfx(bool mute) => Instance.IsSfxMuted = mute;

        private float GetCalculatedBgmVolume(float baseVolume)
        {
            if (isMasterMuted || isBgmMuted) return 0f;
            return baseVolume * bgmVolume * masterVolume;
        }

        private float GetCalculatedSfxVolume(float baseVolume)
        {
            if (isMasterMuted || isSfxMuted) return 0f;
            return baseVolume * sfxVolume * masterVolume;
        }

        private void ApplyVolumeSettings()
        {
            // BGM 볼륨 갱신 (Fade 진행 중이 아닐 때)
            if (bgmSource != null && bgmFadeCoroutine == null)
            {
                bgmSource.volume = GetCalculatedBgmVolume(currentBgmBaseVolume);
            }

            // 재생 중인 SFX 볼륨 갱신
            foreach (var entry in sfxPool)
            {
                if (entry.source != null && entry.source.isPlaying)
                {
                    entry.source.volume = GetCalculatedSfxVolume(entry.baseVolume);
                }
            }
        }

        #endregion

        #region Clip Resolution

        private AudioClip ResolveClip(AudioType type, out AudioDefinition definition)
        {
            definition = null;

            if (audioDataSO != null && audioDataSO.TryGetDefinition(type, out definition))
            {
                if (definition.clip != null)
                {
                    return definition.clip;
                }

                if (!string.IsNullOrEmpty(definition.address))
                {
                    // 1. 커스텀 로더 (Addressables 연동 등)
                    if (CustomClipLoader != null)
                    {
                        var customClip = CustomClipLoader.Invoke(definition.address);
                        if (customClip != null) return customClip;
                    }

                    // 2. Resources fallback
                    var resourceClip = Resources.Load<AudioClip>(definition.address);
                    if (resourceClip != null) return resourceClip;
                }
            }

            return null;
        }

        #endregion
    }
}
