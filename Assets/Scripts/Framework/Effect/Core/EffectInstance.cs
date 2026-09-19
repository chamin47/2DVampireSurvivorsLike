using UnityEngine;

namespace Framework.Effect
{
    /// <summary>
    /// 재생 중인 이펙트 인스턴스를 래핑하고 런타임 제어(정지, 위치/부모 변경 등)를 제공하는 핸들 클래스입니다.
    /// </summary>
    public class EffectInstance
    {
        private readonly EffectType type;
        private readonly GameObject gameObject;
        private readonly EffectPlayer player;

        public EffectType Type => type;
        public GameObject GameObject => gameObject;
        public EffectPlayer Player => player;

        /// <summary>
        /// 이펙트가 현재 활성화되어 재생 중인지 여부입니다.
        /// </summary>
        public bool IsPlaying => player != null && player.IsPlaying && gameObject != null && gameObject.activeInHierarchy;

        /// <summary>
        /// 인스턴스가 유효한 GameObject 및 Player를 가리키고 있는지 확인합니다.
        /// </summary>
        public bool IsValid => gameObject != null && player != null;

        public EffectInstance(EffectType type, GameObject gameObject, EffectPlayer player)
        {
            this.type = type;
            this.gameObject = gameObject;
            this.player = player;
        }

        /// <summary>
        /// 이펙트 재생을 즉시 정지하고 Pool로 반환합니다.
        /// </summary>
        public void Stop()
        {
            if (player != null)
            {
                player.Stop();
            }
        }

        /// <summary>
        /// 이펙트 일시 정지
        /// </summary>
        public void Pause()
        {
            if (player != null)
            {
                player.Pause();
            }
        }

        /// <summary>
        /// 이펙트 재개
        /// </summary>
        public void Resume()
        {
            if (player != null)
            {
                player.Resume();
            }
        }

        /// <summary>
        /// 월드 위치 지정
        /// </summary>
        public void SetPosition(Vector3 position)
        {
            if (gameObject != null)
            {
                gameObject.transform.position = position;
            }
        }

        /// <summary>
        /// 부모 Transform 변경
        /// </summary>
        public void SetParent(Transform parent, bool worldPositionStays = true)
        {
            if (gameObject != null)
            {
                gameObject.transform.SetParent(parent, worldPositionStays);
            }
        }

        /// <summary>
        /// 스케일 설정
        /// </summary>
        public void SetScale(Vector3 scale)
        {
            if (gameObject != null)
            {
                gameObject.transform.localScale = scale;
            }
        }
    }
}
