using System.Collections;
using Framework.Asset;
using Framework.Audio;
using UnityEngine;

namespace DawnFarm
{
    [RequireComponent(typeof(SpriteRenderer), typeof(SpriteFlipbook))]
    public sealed class FarmEnemy : MonoBehaviour
    {
        private static float lastHitSoundTime = -10f;
        private IFarmGameSession session;
        private EnemyDefinition definition;
        private SpriteFlipbook animator;
        private SpriteRenderer spriteRenderer;
        private float health;
        private float contactTimer;
        private float rangedTimer;
        private bool alive;

        public EnemyKind Kind => definition != null ? definition.kind : EnemyKind.Zombie;
        public bool IsAlive => alive;
        public float CollisionRadius => Kind == EnemyKind.Reaper ? 0.85f : 0.42f;

        private void Awake()
        {
            animator = GetComponent<SpriteFlipbook>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Configure(IFarmGameSession gameSession, EnemyDefinition enemy, Vector3 position, float healthScale = 1f)
        {
            session = gameSession;
            definition = enemy;
            transform.position = position;
            transform.localScale = Vector3.one * (enemy.kind == EnemyKind.Reaper ? 2.2f : 1f);
            health = enemy.maxHealth * healthScale;
            alive = true;
            contactTimer = 0f;
            rangedTimer = Random.Range(0.4f, Mathf.Max(0.5f, enemy.attackCooldown));
            animator.Play(enemy.runSprites, enemy.kind == EnemyKind.Runner ? 12f : 8f);
            session.RegisterEnemy(this);
        }

        private void Update()
        {
            if (!alive || session == null || session.State != FarmGameState.Playing || session.Player == null) return;
            Vector2 toPlayer = session.Player.transform.position - transform.position;
            float distance = toPlayer.magnitude;
            if (toPlayer.x != 0f) spriteRenderer.flipX = toPlayer.x < 0f;
            contactTimer -= Time.deltaTime;
            rangedTimer -= Time.deltaTime;

            if (definition.ranged && distance < 7.5f)
            {
                if (distance > 4.2f) transform.position += (Vector3)(toPlayer.normalized * definition.moveSpeed * Time.deltaTime);
                if (rangedTimer <= 0f)
                {
                    rangedTimer = definition.attackCooldown;
                    session.SpawnProjectile(transform.position, toPlayer.normalized, true, definition.touchDamage, 4.5f, 0, session.Config.enemyBulletSprite);
                }
            }
            else
            {
                transform.position += (Vector3)(toPlayer.normalized * definition.moveSpeed * Time.deltaTime);
            }

            if (distance < CollisionRadius + 0.38f && contactTimer <= 0f)
            {
                contactTimer = 0.8f;
                session.Player.TakeDamage(definition.touchDamage);
            }
        }

        public void TakeDamage(float damage)
        {
            if (!alive || damage <= 0f) return;
            health -= damage;
            GameServices.Get<FarmEffectService>()?.Burst(transform.position, new Color(1f, 0.82f, 0.28f), 5);
            GameServices.Get<FarmEffectService>()?.Flash(spriteRenderer, Color.white, 0.08f);
            if (Time.realtimeSinceStartup - lastHitSoundTime >= 0.08f)
            {
                AudioClip hitClip = Random.value < 0.5f ? session.Config.hit : session.Config.hitAlternate;
                if (hitClip == null) hitClip = session.Config.hit ?? session.Config.hitAlternate;
                if (hitClip != null)
                {
                    lastHitSoundTime = Time.realtimeSinceStartup;
                    AudioPlayer.PlaySfx(hitClip, 0.24f, Random.Range(0.96f, 1.06f));
                }
            }
            if (health <= 0f) StartCoroutine(DieRoutine());
        }

        private IEnumerator DieRoutine()
        {
            alive = false;
            session.UnregisterEnemy(this);
            animator.Show(definition.deadSprite);
            if (session.Config.dead != null) AudioPlayer.PlaySfx(session.Config.dead, 0.35f, Random.Range(0.92f, 1.08f));
            session.EnemyDefeated(this);
            yield return new WaitForSeconds(0.18f);
            AssetLoader.Despawn(gameObject);
        }

        private void OnDisable()
        {
            if (alive && session != null) session.UnregisterEnemy(this);
            alive = false;
        }
    }
}
