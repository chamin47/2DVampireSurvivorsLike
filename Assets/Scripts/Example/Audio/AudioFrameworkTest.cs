using System.Collections;
using UnityEngine;
using Framework.Audio;

namespace Example.Audio
{
    /// <summary>
    /// Audio System 프레임워크 동작 검증 및 예제용 MonoBehaviour 스크립트입니다.
    /// </summary>
    public class AudioFrameworkTest : MonoBehaviour
    {
        [Header("Test Clips (Optional for Manual Test)")]
        [SerializeField] private AudioClip bgmMainClip;
        [SerializeField] private AudioClip bgmBattleClip;
        [SerializeField] private AudioClip sfxAttackClip;
        [SerializeField] private AudioClip sfxHitClip;
        [SerializeField] private AudioClip sfxClickClip;

        private void Start()
        {
            Debug.Log("[AudioFrameworkTest] Starting Audio System Test Sequence...");

            // 1. AudioDataSO 데이터 동적 구성을 위한 테스트용 ScriptableObject 생성
            var testDataSO = ScriptableObject.CreateInstance<AudioDataSO>();
            
            if (bgmMainClip != null)
                testDataSO.RegisterDefinition(new AudioDefinition(Framework.Audio.AudioType.MainTheme, bgmMainClip, 1.0f));
            if (bgmBattleClip != null)
                testDataSO.RegisterDefinition(new AudioDefinition(Framework.Audio.AudioType.Battle, bgmBattleClip, 1.0f));
            if (sfxAttackClip != null)
                testDataSO.RegisterDefinition(new AudioDefinition(Framework.Audio.AudioType.Attack, sfxAttackClip, 0.8f, 1.2f));
            if (sfxHitClip != null)
                testDataSO.RegisterDefinition(new AudioDefinition(Framework.Audio.AudioType.Hit, sfxHitClip, 1.0f));
            if (sfxClickClip != null)
                testDataSO.RegisterDefinition(new AudioDefinition(Framework.Audio.AudioType.ButtonClick, sfxClickClip, 0.5f));
            AudioPlayer.Instance.SetAudioData(testDataSO);

            StartCoroutine(RunTestSequence());
        }

        private IEnumerator RunTestSequence()
        {
            // -------------------------------------------------------------
            // Step 1. BGM 재생 및 Loop 테스트
            // -------------------------------------------------------------
            Debug.Log("[AudioFrameworkTest] Step 1: Playing BGM (MainTheme, fadeDuration: 1.0s)...");
            AudioPlayer.PlayBgm(Framework.Audio.AudioType.MainTheme, loop: true, fadeDuration: 1.0f);
            yield return new WaitForSeconds(2.0f);

            // -------------------------------------------------------------
            // Step 2. SFX 동시 중첩 재생 테스트
            // -------------------------------------------------------------
            Debug.Log("[AudioFrameworkTest] Step 2: Playing Multiple SFX simultaneously (Attack, Hit, ButtonClick)...");
            AudioPlayer.PlaySfx(Framework.Audio.AudioType.Attack);
            AudioPlayer.PlaySfx(Framework.Audio.AudioType.Hit);
            AudioPlayer.PlaySfx(Framework.Audio.AudioType.ButtonClick);
            yield return new WaitForSeconds(1.5f);

            // -------------------------------------------------------------
            // Step 3. BGM 교체 및 Fade Out -> Fade In 테스트
            // -------------------------------------------------------------
            Debug.Log("[AudioFrameworkTest] Step 3: Transitioning BGM from MainTheme to Battle (fadeDuration: 2.0s)...");
            AudioPlayer.PlayBgm(Framework.Audio.AudioType.Battle, loop: true, fadeDuration: 2.0f);
            yield return new WaitForSeconds(3.0f);

            // -------------------------------------------------------------
            // Step 4. Mute / Volume 테스트
            // -------------------------------------------------------------
            Debug.Log("[AudioFrameworkTest] Step 4: Muting SFX and changing BGM Volume to 0.5...");
            AudioPlayer.MuteSfx(true);
            AudioPlayer.SetBgmVolume(0.5f);

            Debug.Log("[AudioFrameworkTest] Playing SFX while SFX is muted (should not be audible)...");
            AudioPlayer.PlaySfx(Framework.Audio.AudioType.Attack);
            yield return new WaitForSeconds(1.0f);

            Debug.Log("[AudioFrameworkTest] Unmuting SFX and restoring BGM Volume...");
            AudioPlayer.MuteSfx(false);
            AudioPlayer.SetBgmVolume(1.0f);
            yield return new WaitForSeconds(1.0f);

            // -------------------------------------------------------------
            // Step 5. Specific SFX Stop 및 BGM Fade Out Stop
            // -------------------------------------------------------------
            Debug.Log("[AudioFrameworkTest] Step 5: Playing Attack SFX and stopping Attack SFX specifically...");
            AudioPlayer.PlaySfx(Framework.Audio.AudioType.Attack);
            yield return new WaitForSeconds(0.1f);
            AudioPlayer.StopSfx(Framework.Audio.AudioType.Attack);

            Debug.Log("[AudioFrameworkTest] Stopping BGM with 1.5s Fade Out...");
            AudioPlayer.StopBgm(fadeDuration: 1.5f);
            yield return new WaitForSeconds(2.0f);

            Debug.Log("[AudioFrameworkTest] Audio System Test Sequence Completed Successfully!");
        }
    }
}
