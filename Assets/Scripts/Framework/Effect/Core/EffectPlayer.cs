using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.Asset;

namespace Framework.Effect
{
    /// <summary>
    /// 개별 Effect Prefab에 부착되어 내부 ParticleSystem, Animator, Animation 등의 재생,
    /// 지속 시간 자동 계산 및 풀 반환(AutoRelease)을 담당하는 이펙트 생명주기 관리 컴포넌트입니다.
    /// </summary>
    public class EffectPlayer : MonoBehaviour
    {
        private EffectDefinition definition;
        private EffectInstance instanceHandle;
        private Action<EffectPlayer> onCompleteCallback;

        private ParticleSystem[] particleSystems;
        private Animator[] animators;
        private Animation[] legacyAnimations;
        private AudioSource[] audioSources;

        private bool isPlaying;
        private float currentDuration;
        private Coroutine autoReleaseCoroutine;

        public EffectDefinition Definition => definition;
        public EffectInstance InstanceHandle => instanceHandle;
        public bool IsPlaying => isPlaying;
        public float CurrentDuration => currentDuration;

        private void Awake()
        {
            CacheComponents();
        }

        /// <summary>
        /// 이펙트 내부 구성 요소를 찾아 캐싱합니다.
        /// </summary>
        public void CacheComponents()
        {
            particleSystems = GetComponentsInChildren<ParticleSystem>(true);
            animators = GetComponentsInChildren<Animator>(true);
            legacyAnimations = GetComponentsInChildren<Animation>(true);
            audioSources = GetComponentsInChildren<AudioSource>(true);
        }

        /// <summary>
        /// 이펙트 인스턴스 초기화 정보를 전달받습니다.
        /// </summary>
        public void Init(EffectDefinition definition, EffectInstance instanceHandle, Action<EffectPlayer> onCompleteCallback = null)
        {
            this.definition = definition;
            this.instanceHandle = instanceHandle;
            this.onCompleteCallback = onCompleteCallback;

            if (particleSystems == null || particleSystems.Length == 0)
            {
                CacheComponents();
            }
        }

        /// <summary>
        /// 이펙트 재생을 시작합니다.
        /// </summary>
        public void Play()
        {
            isPlaying = true;
            gameObject.SetActive(true);

            // 1. Particle Systems 재생
            if (particleSystems != null)
            {
                foreach (var ps in particleSystems)
                {
                    if (ps == null) continue;
                    ps.Clear(true);
                    ps.Play(true);
                }
            }

            // 2. Animators 재생
            if (animators != null)
            {
                foreach (var anim in animators)
                {
                    if (anim == null) continue;
                    anim.enabled = true;
                    anim.Rebind();
                    anim.Update(0f);
                }
            }

            // 3. Legacy Animations 재생
            if (legacyAnimations != null)
            {
                foreach (var anim in legacyAnimations)
                {
                    if (anim == null) continue;
                    anim.Rewind();
                    anim.Play();
                }
            }

            // 4. AudioSources 재생
            if (audioSources != null)
            {
                foreach (var audio in audioSources)
                {
                    if (audio == null) continue;
                    audio.Play();
                }
            }

            // 5. Duration 계산
            currentDuration = CalculateDuration();

            // 6. Auto Release 코루틴 시작
            if (autoReleaseCoroutine != null)
            {
                StopCoroutine(autoReleaseCoroutine);
                autoReleaseCoroutine = null;
            }

            if (definition != null && definition.autoRelease)
            {
                autoReleaseCoroutine = StartCoroutine(CoAutoRelease(currentDuration));
            }
        }

        /// <summary>
        /// 이펙트 재생을 정지하고 반환합니다.
        /// </summary>
        public void Stop()
        {
            if (!isPlaying && !gameObject.activeInHierarchy) return;

            isPlaying = false;

            if (autoReleaseCoroutine != null)
            {
                StopCoroutine(autoReleaseCoroutine);
                autoReleaseCoroutine = null;
            }

            // Component 정지
            if (particleSystems != null)
            {
                foreach (var ps in particleSystems)
                {
                    if (ps == null) continue;
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                }
            }

            if (audioSources != null)
            {
                foreach (var audio in audioSources)
                {
                    if (audio == null) continue;
                    audio.Stop();
                }
            }

            // 콜백 호출 및 풀 반환 처리
            onCompleteCallback?.Invoke(this);
        }

        /// <summary>
        /// 이펙트 일시 정지
        /// </summary>
        public void Pause()
        {
            if (!isPlaying) return;

            if (particleSystems != null)
            {
                foreach (var ps in particleSystems)
                {
                    if (ps != null) ps.Pause(true);
                }
            }

            if (animators != null)
            {
                foreach (var anim in animators)
                {
                    if (anim != null) anim.speed = 0f;
                }
            }
        }

        /// <summary>
        /// 이펙트 일시 정지 해제
        /// </summary>
        public void Resume()
        {
            if (!isPlaying) return;

            if (particleSystems != null)
            {
                foreach (var ps in particleSystems)
                {
                    if (ps != null) ps.Play(true);
                }
            }

            if (animators != null)
            {
                foreach (var anim in animators)
                {
                    if (anim != null) anim.speed = 1f;
                }
            }
        }

        private float CalculateDuration()
        {
            if (definition != null && definition.duration > 0f)
            {
                return definition.duration;
            }

            float maxDuration = 0f;

            // ParticleSystem 길이에 따른 자동 지속시간 계산
            if (particleSystems != null && particleSystems.Length > 0)
            {
                foreach (var ps in particleSystems)
                {
                    if (ps == null) continue;
                    var main = ps.main;
                    float duration = main.duration + main.startDelay.constantMax + main.startLifetime.constantMax;
                    if (duration > maxDuration)
                    {
                        maxDuration = duration;
                    }
                }
            }

            // 기본값 설정 (ParticleSystem이 없고 duration 설정도 없으면 2.0초)
            return maxDuration > 0f ? maxDuration : 2.0f;
        }

        private IEnumerator CoAutoRelease(float delay)
        {
            yield return new WaitForSeconds(delay);
            Stop();
        }

        private void OnDisable()
        {
            if (autoReleaseCoroutine != null)
            {
                StopCoroutine(autoReleaseCoroutine);
                autoReleaseCoroutine = null;
            }
            isPlaying = false;
        }
    }
}
