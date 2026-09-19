using System.Collections;
using UnityEngine;

namespace DawnFarm
{
    public sealed class FarmEffectService : MonoBehaviour
    {
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
                var particle = GameObject.CreatePrimitive(PrimitiveType.Quad);
                particle.name = "Spark";
                particle.transform.SetParent(root.transform, false);
                particle.transform.localScale = Vector3.one * 0.06f;
                var renderer = particle.GetComponent<MeshRenderer>();
                renderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Color"));
                renderer.material.color = color;
                Destroy(particle.GetComponent<Collider>());
                particle.AddComponent<BurstParticle>().Launch(Random.insideUnitCircle.normalized, Random.Range(2f, 4f));
            }
            Destroy(root, 0.35f);
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
