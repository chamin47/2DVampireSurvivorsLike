using System.Collections;
using UnityEngine;

namespace DawnFarm
{
    public sealed class FarmEffectService : MonoBehaviour
    {
        private static Sprite burstSprite;

        public void Flash(SpriteRenderer renderer, Color color, float duration)
        {
            if (renderer != null) StartCoroutine(FlashRoutine(renderer, color, duration));
        }

        private static IEnumerator FlashRoutine(SpriteRenderer renderer, Color color, float duration)
        {
            Color original = renderer.color;
            renderer.color = color;
            yield return new WaitForSeconds(duration);
            if (renderer != null) renderer.color = original;
        }

        public void Burst(Vector3 position, Color color, int count)
        {
            var root = new GameObject("HitBurst");
            root.transform.position = position;
            for (int i = 0; i < count; i++)
            {
                var particle = new GameObject("Spark");
                particle.name = "Spark";
                particle.transform.SetParent(root.transform, false);
                particle.transform.localScale = Vector3.one * 0.06f;
                particle.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
                var renderer = particle.AddComponent<SpriteRenderer>();
                renderer.sprite = GetBurstSprite();
                renderer.color = color;
                renderer.sortingOrder = 40;
                particle.AddComponent<BurstParticle>().Launch(Random.insideUnitCircle.normalized, Random.Range(2f, 4f));
            }
            Destroy(root, 0.35f);
        }

        private static Sprite GetBurstSprite()
        {
            if (burstSprite != null) return burstSprite;

            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
            {
                name = "Runtime Hit Pixel",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            texture.Apply(false, true);
            burstSprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 2f);
            burstSprite.name = "Runtime Hit Pixel";
            burstSprite.hideFlags = HideFlags.HideAndDontSave;
            return burstSprite;
        }
    }

    public sealed class BurstParticle : MonoBehaviour
    {
        private Vector2 velocity;
        public void Launch(Vector2 direction, float speed) => velocity = direction * speed;
        private void Update()
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);
            transform.localScale *= 1f - Time.deltaTime * 4f;
        }
    }
}
