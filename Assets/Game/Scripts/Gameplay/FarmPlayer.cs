using System.Collections;
using Framework.Event;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DawnFarm
{
    [RequireComponent(typeof(SpriteRenderer), typeof(SpriteFlipbook))]
    public sealed class FarmPlayer : MonoBehaviour
    {
        private IFarmGameSession session;
        private CharacterDefinition definition;
        private SpriteFlipbook animator;
        private SpriteRenderer spriteRenderer;
        private float health;
        private float maxHealth;
        private float moveSpeed;
        private float invulnerability;
        private bool dead;
        private Sprite[] currentAnimation;

        public float Health => health;
        public float MaxHealth => maxHealth;
        public float PickupRadius { get; private set; }
        public float DamageMultiplier { get; private set; }
        public float CooldownMultiplier { get; private set; }
        public bool IsAlive => !dead;

        private void Awake()
        {
            animator = GetComponent<SpriteFlipbook>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Configure(IFarmGameSession gameSession, CharacterDefinition character)
        {
            session = gameSession;
            definition = character;
            dead = false;
            DamageMultiplier = character != null ? character.damageMultiplier : 1f;
            CooldownMultiplier = character != null ? character.cooldownMultiplier : 1f;
            moveSpeed = session.Config.baseMoveSpeed * (character != null ? character.moveSpeedMultiplier : 1f);
            maxHealth = session.Config.baseMaxHealth * (character != null ? character.maxHealthMultiplier : 1f);
            health = maxHealth;
            PickupRadius = session.Config.basePickupRadius;
            transform.localScale = Vector3.one * 1.15f;
            PlayAnimation(character?.standSprites, 5f);
            EventBus.Publish(new HealthChangedEvent(health, maxHealth));
        }

        private void Update()
        {
            if (dead || session == null || session.State != FarmGameState.Playing) return;
            invulnerability -= Time.deltaTime;
            Vector2 input = ReadMovement();
            if (input.sqrMagnitude > 1f) input.Normalize();
            transform.position += (Vector3)(input * moveSpeed * Time.deltaTime);
            if (input.x != 0f) spriteRenderer.flipX = input.x < 0f;
            PlayAnimation(input.sqrMagnitude > 0.01f ? definition?.runSprites : definition?.standSprites, input.sqrMagnitude > 0.01f ? 9f : 5f);
        }

        private static Vector2 ReadMovement()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null) return Vector2.zero;
            float x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
            float y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);
            return new Vector2(x, y);
#else
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
        }

        private void PlayAnimation(Sprite[] frames, float fps)
        {
            if (frames == null || frames.Length == 0 || ReferenceEquals(frames, currentAnimation)) return;
            currentAnimation = frames;
            animator.Play(frames, fps);
        }

        public void TakeDamage(float amount)
        {
            if (dead || invulnerability > 0f || amount <= 0f) return;
            invulnerability = 0.35f;
            health = Mathf.Max(0f, health - amount);
            EventBus.Publish(new HealthChangedEvent(health, maxHealth));
            GameServices.Get<FarmEffectService>()?.Flash(spriteRenderer, new Color(1f, 0.35f, 0.35f), 0.12f);
            if (health <= 0f) StartCoroutine(DieRoutine());
        }

        public void Heal(float amount)
        {
            health = Mathf.Min(maxHealth, health + Mathf.Max(0f, amount));
            EventBus.Publish(new HealthChangedEvent(health, maxHealth));
        }

        public void AddMoveSpeed(float percent) => moveSpeed *= 1f + percent;
        public void AddDamage(float percent) => DamageMultiplier *= 1f + percent;
        public void ReduceCooldown(float percent) => CooldownMultiplier *= Mathf.Max(0.35f, 1f - percent);
        public void AddPickupRadius(float amount) => PickupRadius += amount;

        public void AddMaxHealth(float percent)
        {
            float oldMax = maxHealth;
            maxHealth *= 1f + percent;
            health += maxHealth - oldMax;
            EventBus.Publish(new HealthChangedEvent(health, maxHealth));
        }

        private IEnumerator DieRoutine()
        {
            dead = true;
            if (definition?.deadSprites != null && definition.deadSprites.Length > 0) animator.Play(definition.deadSprites, 5f);
            yield return new WaitForSecondsRealtime(0.45f);
            session?.EndGame(false);
        }
    }
}
