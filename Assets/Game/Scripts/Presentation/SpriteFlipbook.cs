using UnityEngine;

namespace DawnFarm
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SpriteFlipbook : MonoBehaviour
    {
        [SerializeField] private float framesPerSecond = 8f;
        private SpriteRenderer spriteRenderer;
        private Sprite[] frames;
        private float time;
        private bool playing;

        public SpriteRenderer Renderer => spriteRenderer != null ? spriteRenderer : spriteRenderer = GetComponent<SpriteRenderer>();

        private void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

        public void Play(Sprite[] newFrames, float fps = 8f)
        {
            frames = newFrames;
            framesPerSecond = Mathf.Max(1f, fps);
            time = 0f;
            playing = frames != null && frames.Length > 0;
            if (playing) Renderer.sprite = frames[0];
        }

        public void Show(Sprite sprite)
        {
            playing = false;
            frames = null;
            Renderer.sprite = sprite;
        }

        private void Update()
        {
            if (!playing || frames == null || frames.Length < 2) return;
            time += Time.deltaTime * framesPerSecond;
            Renderer.sprite = frames[Mathf.FloorToInt(time) % frames.Length];
        }
    }
}
