using System;
using System.Collections.Generic;
using Framework.Asset;
using Framework.Audio;
using UnityEngine;

namespace DawnFarm
{
    public readonly struct UpgradeOption
    {
        public readonly UpgradeKind Kind;
        public readonly int NextLevel;
        public readonly string KoreanTitle;
        public readonly string EnglishTitle;
        public readonly string KoreanDescription;
        public readonly string EnglishDescription;

        public UpgradeOption(UpgradeKind kind, int nextLevel, string koTitle, string enTitle, string koDescription, string enDescription)
        {
            Kind = kind;
            NextLevel = nextLevel;
            KoreanTitle = koTitle;
            EnglishTitle = enTitle;
            KoreanDescription = koDescription;
            EnglishDescription = enDescription;
        }
    }

    public sealed class FarmWeaponSystem : MonoBehaviour
    {
        private readonly Dictionary<UpgradeKind, int> levels = new Dictionary<UpgradeKind, int>();
        private readonly List<GameObject> orbitVisuals = new List<GameObject>();
        private IFarmGameSession session;
        private FarmPlayer player;
        private float scytheTimer;
        private float seedTimer;
        private float orbitDamageTimer;
        private float orbitAngle;

        public void Configure(IFarmGameSession gameSession, FarmPlayer owner)
        {
            session = gameSession;
            player = owner;
            levels.Clear();
            foreach (UpgradeKind kind in Enum.GetValues(typeof(UpgradeKind))) levels[kind] = 0;
            levels[UpgradeKind.Scythe] = 1;
            scytheTimer = 0.15f;
            seedTimer = 0.8f;
            orbitDamageTimer = 0f;
            ClearOrbitVisuals();
        }

        private void Update()
        {
            if (session == null || player == null || session.State != FarmGameState.Playing) return;
            float cooldown = player.CooldownMultiplier;
            scytheTimer -= Time.deltaTime;
            seedTimer -= Time.deltaTime;
            orbitDamageTimer -= Time.deltaTime;
            if (levels[UpgradeKind.Scythe] > 0 && scytheTimer <= 0f)
            {
                ScytheAttack(levels[UpgradeKind.Scythe]);
                scytheTimer = Mathf.Max(0.28f, (1.15f - levels[UpgradeKind.Scythe] * 0.07f) * cooldown);
            }
            if (levels[UpgradeKind.SeedGun] > 0 && seedTimer <= 0f)
            {
                SeedAttack(levels[UpgradeKind.SeedGun]);
                seedTimer = Mathf.Max(0.18f, (0.88f - levels[UpgradeKind.SeedGun] * 0.09f) * cooldown);
            }
            if (levels[UpgradeKind.OrbitingPickaxe] > 0) UpdateOrbit(levels[UpgradeKind.OrbitingPickaxe]);
        }

        private void ScytheAttack(int level)
        {
            FarmEnemy target = session.FindNearestEnemy(player.transform.position);
            if (target == null) return;
            Vector2 direction = (target.transform.position - player.transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float radius = 1.65f + level * 0.18f;
            float damage = (14f + level * 5f) * player.DamageMultiplier;
            var visual = session.SpawnWeaponVisual(player.transform.position + (Vector3)(direction * 0.8f), session.Config.scytheSprite);
            if (visual != null)
            {
                visual.transform.rotation = Quaternion.Euler(0f, 0f, angle - 30f);
                visual.GetComponent<TimedWeaponVisual>()?.Configure(session.Config.scytheSprite, 0.18f, 1.35f + level * 0.1f, 420f);
            }
            DamageArc(direction, radius, damage, false);
            if (level >= 3) DamageArc(-direction, radius, damage * 0.85f, true);
            if (session.Config.melee != null) AudioPlayer.PlaySfx(session.Config.melee, 0.32f, UnityEngine.Random.Range(0.94f, 1.06f));
        }

        private void DamageArc(Vector2 direction, float radius, float damage, bool reverse)
        {
            var enemies = session.Enemies;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;
                Vector2 delta = enemy.transform.position - player.transform.position;
                if (delta.sqrMagnitude <= radius * radius && (delta.normalized == Vector2.zero || Vector2.Dot(direction, delta.normalized) > (reverse ? 0.15f : -0.05f)))
                    enemy.TakeDamage(damage);
            }
        }

        private void SeedAttack(int level)
        {
            FarmEnemy target = session.FindNearestEnemy(player.transform.position);
            if (target == null) return;
            Vector2 direction = (target.transform.position - player.transform.position).normalized;
            int count = level >= 4 ? 3 : level >= 2 ? 2 : 1;
            for (int i = 0; i < count; i++)
            {
                float offset = (i - (count - 1) * 0.5f) * 9f;
                Vector2 shotDirection = Quaternion.Euler(0f, 0f, offset) * direction;
                session.SpawnProjectile(player.transform.position, shotDirection, false, (9f + level * 3f) * player.DamageMultiplier, 9f, level >= 3 ? 2 : level - 1, session.Config.playerBulletSprite);
            }
            if (session.Config.ranged != null) AudioPlayer.PlaySfx(session.Config.ranged, 0.28f, UnityEngine.Random.Range(0.96f, 1.08f));
        }

        private void UpdateOrbit(int level)
        {
            int count = 1 + (level - 1) / 2;
            while (orbitVisuals.Count < count)
            {
                var visual = session.SpawnWeaponVisual(player.transform.position, session.Config.pickaxeSprite);
                if (visual == null) break;
                visual.GetComponent<TimedWeaponVisual>()?.Configure(session.Config.pickaxeSprite, 9999f, 0.9f + level * 0.05f, 360f);
                orbitVisuals.Add(visual);
            }
            orbitAngle += Time.deltaTime * (105f + level * 14f);
            float radius = 1.55f + level * 0.08f;
            for (int i = 0; i < orbitVisuals.Count; i++)
            {
                if (orbitVisuals[i] == null) continue;
                float angle = (orbitAngle + i * 360f / orbitVisuals.Count) * Mathf.Deg2Rad;
                orbitVisuals[i].transform.position = player.transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            if (orbitDamageTimer > 0f) return;
            orbitDamageTimer = Mathf.Max(0.15f, 0.42f - level * 0.035f);
            float hitRadius = 0.48f + level * 0.04f;
            foreach (var visual in orbitVisuals)
            {
                if (visual == null) continue;
                for (int i = session.Enemies.Count - 1; i >= 0; i--)
                {
                    var enemy = session.Enemies[i];
                    if (enemy != null && enemy.IsAlive && ((Vector2)(enemy.transform.position - visual.transform.position)).sqrMagnitude < hitRadius * hitRadius)
                        enemy.TakeDamage((7f + level * 2.5f) * player.DamageMultiplier);
                }
            }
        }

        public UpgradeOption[] CreateChoices(int count = 3)
        {
            var candidates = new List<UpgradeKind>();
            foreach (UpgradeKind kind in Enum.GetValues(typeof(UpgradeKind))) if (levels[kind] < 5) candidates.Add(kind);
            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int swap = UnityEngine.Random.Range(0, i + 1);
                (candidates[i], candidates[swap]) = (candidates[swap], candidates[i]);
            }
            int resultCount = Mathf.Min(count, candidates.Count);
            var result = new UpgradeOption[resultCount];
            for (int i = 0; i < resultCount; i++) result[i] = Describe(candidates[i], levels[candidates[i]] + 1);
            return result;
        }

        public void ApplyUpgrade(UpgradeKind kind)
        {
            if (!levels.ContainsKey(kind) || levels[kind] >= 5) return;
            levels[kind]++;
            switch (kind)
            {
                case UpgradeKind.Power: player.AddDamage(0.12f); break;
                case UpgradeKind.MoveSpeed: player.AddMoveSpeed(0.08f); break;
                case UpgradeKind.Cooldown: player.ReduceCooldown(0.08f); break;
                case UpgradeKind.PickupRadius: player.AddPickupRadius(0.55f); break;
                case UpgradeKind.MaxHealth: player.AddMaxHealth(0.12f); break;
            }
        }

        public void UpgradeRandomOwnedWeapon()
        {
            var owned = new List<UpgradeKind>();
            foreach (var kind in new[] { UpgradeKind.Scythe, UpgradeKind.SeedGun, UpgradeKind.OrbitingPickaxe })
                if (levels[kind] > 0 && levels[kind] < 5) owned.Add(kind);
            if (owned.Count == 0) return;
            ApplyUpgrade(owned[UnityEngine.Random.Range(0, owned.Count)]);
        }

        private static UpgradeOption Describe(UpgradeKind kind, int level)
        {
            switch (kind)
            {
                case UpgradeKind.Scythe: return new UpgradeOption(kind, level, "농부의 낫", "FARMER'S SCYTHE", "피해·범위·양방향 공격 강화", "More damage, range and dual swings");
                case UpgradeKind.SeedGun: return new UpgradeOption(kind, level, "씨앗총", "SEED GUN", "연사·관통·탄환 수 강화", "Faster, piercing, multi-shot seeds");
                case UpgradeKind.OrbitingPickaxe: return new UpgradeOption(kind, level, "회전 곡괭이", "ORBITING PICKAXE", "개수·회전·크기 강화", "More, faster and larger pickaxes");
                case UpgradeKind.Power: return new UpgradeOption(kind, level, "힘", "POWER", "모든 공격력 +12%", "All damage +12%");
                case UpgradeKind.MoveSpeed: return new UpgradeOption(kind, level, "장화", "BOOTS", "이동 속도 +8%", "Move speed +8%");
                case UpgradeKind.Cooldown: return new UpgradeOption(kind, level, "민첩한 손", "QUICK HANDS", "재사용 대기시간 -8%", "Cooldown -8%");
                case UpgradeKind.PickupRadius: return new UpgradeOption(kind, level, "큰 바구니", "LARGE BASKET", "획득 범위 증가", "Pickup radius increased");
                default: return new UpgradeOption(kind, level, "튼튼한 심장", "HEARTY", "최대 체력 +12%", "Maximum health +12%");
            }
        }

        private void OnDisable() => ClearOrbitVisuals();

        private void ClearOrbitVisuals()
        {
            foreach (var visual in orbitVisuals) if (visual != null) AssetLoader.Despawn(visual);
            orbitVisuals.Clear();
        }
    }
}
