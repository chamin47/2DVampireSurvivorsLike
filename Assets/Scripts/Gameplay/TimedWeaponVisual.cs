using Framework.Asset;
using UnityEngine;

namespace DawnFarm
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TimedWeaponVisual : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private float remaining;
        private float spin;
        private bool active;

        private void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();

        public void Configure(Sprite sprite, float duration, float scale, float spinSpeed = 0f)
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.white;
            spriteRenderer.flipX = false;
            spriteRenderer.flipY = false;
            remaining = duration;
            spin = spinSpeed;
            transform.localScale = Vector3.one * scale;
            active = true;
        }

        private void Update()
        {
            if (!active) return;
            remaining -= Time.deltaTime;
            transform.Rotate(0f, 0f, spin * Time.deltaTime);
            if (remaining <= 0f)
            {
                active = false;
                AssetLoader.Despawn(gameObject);
            }
        }

        private void OnDisable()
        {
            active = false;
            remaining = 0f;
            spin = 0f;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}
