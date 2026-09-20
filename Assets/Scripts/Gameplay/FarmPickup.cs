using Framework.Asset;
using UnityEngine;

namespace DawnFarm
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class FarmPickup : MonoBehaviour
    {
        private IFarmGameSession session;
        private PickupKind kind;
        private int amount;
        private float baseScale;
        private float pulseOffset;
        private bool active;

        public void Configure(IFarmGameSession gameSession, PickupKind pickupKind, int value, Vector3 position, Sprite sprite)
        {
            session = gameSession;
            kind = pickupKind;
            amount = value;
            transform.position = position;
            baseScale = kind switch
            {
                PickupKind.Experience => 0.55f,
                PickupKind.Health => 0.82f,
                PickupKind.Magnet => 0.9f,
                PickupKind.Chest => 1.05f,
                _ => 0.8f
            };
            transform.localScale = Vector3.one * baseScale;
            pulseOffset = Random.value * Mathf.PI * 2f;

            var renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = kind == PickupKind.Experience
                ? new Color(0.55f, 0.95f, 1f, 1f)
                : Color.white;
            renderer.sortingOrder = kind == PickupKind.Chest ? 12 : 8;
            active = true;
        }

        private void Update()
        {
            if (!active || session == null || session.State != FarmGameState.Playing || session.Player == null) return;
            if (kind == PickupKind.Chest)
                transform.localScale = Vector3.one * baseScale * (1f + Mathf.Sin(Time.unscaledTime * 5f + pulseOffset) * 0.055f);
            Vector2 toPlayer = session.Player.transform.position - transform.position;
            float radius = session.IsMagnetActive && kind == PickupKind.Experience ? 100f : session.Player.PickupRadius;
            if (toPlayer.sqrMagnitude <= radius * radius)
                transform.position += (Vector3)(toPlayer.normalized * (session.IsMagnetActive ? 18f : 8f) * Time.deltaTime);
            if (toPlayer.sqrMagnitude <= 0.28f)
            {
                active = false;
                session.CollectPickup(kind, amount);
                AssetLoader.Despawn(gameObject);
            }
        }

        private void OnDisable()
        {
            active = false;
            baseScale = 0f;
            transform.localScale = Vector3.one;
            var renderer = GetComponent<SpriteRenderer>();
            renderer.color = Color.white;
            renderer.sortingOrder = 8;
        }
    }
}
