using System.Collections.Generic;
using Framework.Asset;
using UnityEngine;

namespace DawnFarm
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class FarmProjectile : MonoBehaviour
    {
        private readonly HashSet<int> hitTargets = new HashSet<int>();
        private IFarmGameSession session;
        private Vector2 direction;
        private bool hostile;
        private float damage;
        private float speed;
        private float lifetime;
        private int penetration;
        private float spinSpeed;
        private bool active;

        public void Configure(IFarmGameSession gameSession, Vector3 position, Vector2 moveDirection, bool isHostile, float projectileDamage, float moveSpeed, int pierce, Sprite sprite)
        {
            session = gameSession;
            transform.position = position;
            direction = moveDirection.sqrMagnitude > 0f ? moveDirection.normalized : Vector2.right;
            hostile = isHostile;
            damage = projectileDamage;
            speed = moveSpeed;
            penetration = Mathf.Max(0, pierce);
            spinSpeed = hostile ? 0f : 540f;
            lifetime = 5f;
            active = true;
            hitTargets.Clear();
            var renderer = GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = hostile ? new Color(1f, 0.45f, 0.45f) : Color.white;
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            transform.localScale = Vector3.one * (hostile ? 0.8f : 1f);
        }

        private void Update()
        {
            if (!active || session == null || session.State != FarmGameState.Playing) return;
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            if (spinSpeed != 0f) transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
            lifetime -= Time.deltaTime;
            if (lifetime <= 0f) { Despawn(); return; }

            if (hostile)
            {
                var player = session.Player;
                if (player != null && ((Vector2)(player.transform.position - transform.position)).sqrMagnitude < 0.28f)
                {
                    player.TakeDamage(damage);
                    Despawn();
                }
                return;
            }

            var enemies = session.Enemies;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive || hitTargets.Contains(enemy.GetInstanceID())) continue;
                float radius = enemy.CollisionRadius + 0.18f;
                if (((Vector2)(enemy.transform.position - transform.position)).sqrMagnitude > radius * radius) continue;
                hitTargets.Add(enemy.GetInstanceID());
                enemy.TakeDamage(damage);
                if (penetration-- <= 0) { Despawn(); break; }
            }
        }

        private void Despawn()
        {
            if (!active) return;
            active = false;
            AssetLoader.Despawn(gameObject);
        }

        private void OnDisable()
        {
            active = false;
            spinSpeed = 0f;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}
