using Framework.Asset;
using UnityEngine;

namespace DawnFarm
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class TimedWeaponVisual : MonoBehaviour
    {
        private float remaining;
        private float spin;
        private bool active;

        public void Configure(Sprite sprite, float duration, float scale, float spinSpeed = 0f)
        {
            GetComponent<SpriteRenderer>().sprite = sprite;
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

        private void OnDisable() => active = false;
    }
}
