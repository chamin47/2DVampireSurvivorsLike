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
        private bool active;

        public void Configure(IFarmGameSession gameSession, PickupKind pickupKind, int value, Vector3 position, Sprite sprite)
        {
            session = gameSession;
            kind = pickupKind;
            amount = value;
            transform.position = position;
            transform.localScale = Vector3.one * (kind == PickupKind.Chest ? 1.25f : 0.8f);
            GetComponent<SpriteRenderer>().sprite = sprite;
            active = true;
        }

        private void Update()
        {
            if (!active || session == null || session.State != FarmGameState.Playing || session.Player == null) return;
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

        private void OnDisable() => active = false;
    }
}
