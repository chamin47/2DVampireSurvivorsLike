using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace DawnFarm
{
    [Serializable]
    internal sealed class FarmBalanceRow
    {
        public string key;
        public string type;
        public string value;
        public string description;
    }

    [Serializable]
    internal sealed class FarmBalanceDocument
    {
        public FarmBalanceRow[] data;
    }

    public static class FarmBalanceRuntime
    {
        private const string ResourcePath = "Tables/FarmBalance";

        public static bool LoadAndApply(FarmGameConfig config)
        {
            TextAsset source = Resources.Load<TextAsset>(ResourcePath);
            if (source == null)
            {
                Debug.LogWarning($"[FarmBalance] Resources/{ResourcePath}.json was not found. ScriptableObject defaults will be used.");
                return false;
            }

            FarmBalanceDocument document;
            try
            {
                document = JsonUtility.FromJson<FarmBalanceDocument>(source.text);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[FarmBalance] JSON parse failed: {exception.Message}. ScriptableObject defaults will be used.");
                return false;
            }

            if (document?.data == null)
            {
                Debug.LogError("[FarmBalance] JSON has no data array. ScriptableObject defaults will be used.");
                return false;
            }

            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (FarmBalanceRow row in document.data)
            {
                if (row == null || string.IsNullOrWhiteSpace(row.key)) continue;
                values[row.key.Trim()] = row.value?.Trim() ?? string.Empty;
            }

            ApplyStage(config, values);
            ApplyWeapons(config, values);
            ApplyCharacters(config, values);
            ApplyEnemies(config, values);
            Debug.Log($"[FarmBalance] Applied {values.Count} Excel values. Stage={config.stageDuration:0.#}s, Spawn={config.baseSpawnInterval:0.###}s, MaxEnemies={config.maxEnemies}.");
            return true;
        }

        private static void ApplyStage(FarmGameConfig c, IReadOnlyDictionary<string, string> v)
        {
            c.stageDuration = Float(v, "StageDuration", c.stageDuration, 10f);
            c.baseSpawnInterval = Float(v, "BaseSpawnInterval", c.baseSpawnInterval, 0.02f);
            c.maxEnemies = Int(v, "MaxEnemies", c.maxEnemies, 1);
            c.chestInterval = Float(v, "ChestInterval", c.chestInterval, 1f);
            c.finalWaveTime = Float(v, "FinalWaveTime", c.finalWaveTime, 1f);
            c.finalWaveCount = Int(v, "FinalWaveCount", c.finalWaveCount, 0);
            c.healthDropChance = Float(v, "HealthDropChance", c.healthDropChance, 0f, 1f);
            c.magnetDropChance = Float(v, "MagnetDropChance", c.magnetDropChance, 0f, 1f);
            c.magnetDuration = Float(v, "MagnetDuration", c.magnetDuration, 0f);
            c.enemyHealthScaleAtEnd = Float(v, "EnemyHealthScaleAtEnd", c.enemyHealthScaleAtEnd, 0f);
            c.baseMaxHealth = Float(v, "BaseMaxHealth", c.baseMaxHealth, 1f);
            c.baseMoveSpeed = Float(v, "BaseMoveSpeed", c.baseMoveSpeed, 0.1f);
            c.basePickupRadius = Float(v, "BasePickupRadius", c.basePickupRadius, 0.1f);
        }

        private static void ApplyWeapons(FarmGameConfig c, IReadOnlyDictionary<string, string> v)
        {
            c.scytheBaseCooldown = Float(v, "ScytheBaseCooldown", c.scytheBaseCooldown, 0.01f);
            c.scytheCooldownPerLevel = Float(v, "ScytheCooldownPerLevel", c.scytheCooldownPerLevel, 0f);
            c.scytheMinCooldown = Float(v, "ScytheMinCooldown", c.scytheMinCooldown, 0.01f);
            c.scytheBaseDamage = Float(v, "ScytheBaseDamage", c.scytheBaseDamage, 0f);
            c.scytheDamagePerLevel = Float(v, "ScytheDamagePerLevel", c.scytheDamagePerLevel, 0f);
            c.scytheBaseRadius = Float(v, "ScytheBaseRadius", c.scytheBaseRadius, 0.1f);
            c.scytheRadiusPerLevel = Float(v, "ScytheRadiusPerLevel", c.scytheRadiusPerLevel, 0f);
            c.seedBaseCooldown = Float(v, "SeedBaseCooldown", c.seedBaseCooldown, 0.01f);
            c.seedCooldownPerLevel = Float(v, "SeedCooldownPerLevel", c.seedCooldownPerLevel, 0f);
            c.seedMinCooldown = Float(v, "SeedMinCooldown", c.seedMinCooldown, 0.01f);
            c.seedBaseDamage = Float(v, "SeedBaseDamage", c.seedBaseDamage, 0f);
            c.seedDamagePerLevel = Float(v, "SeedDamagePerLevel", c.seedDamagePerLevel, 0f);
            c.seedProjectileSpeed = Float(v, "SeedProjectileSpeed", c.seedProjectileSpeed, 0.1f);
            c.orbitBaseDamage = Float(v, "OrbitBaseDamage", c.orbitBaseDamage, 0f);
            c.orbitDamagePerLevel = Float(v, "OrbitDamagePerLevel", c.orbitDamagePerLevel, 0f);
            c.orbitBaseSpeed = Float(v, "OrbitBaseSpeed", c.orbitBaseSpeed, 0f);
            c.orbitSpeedPerLevel = Float(v, "OrbitSpeedPerLevel", c.orbitSpeedPerLevel, 0f);
            c.orbitBaseRadius = Float(v, "OrbitBaseRadius", c.orbitBaseRadius, 0.1f);
            c.orbitRadiusPerLevel = Float(v, "OrbitRadiusPerLevel", c.orbitRadiusPerLevel, 0f);
            c.orbitBaseHitRadius = Float(v, "OrbitBaseHitRadius", c.orbitBaseHitRadius, 0.05f);
            c.orbitHitRadiusPerLevel = Float(v, "OrbitHitRadiusPerLevel", c.orbitHitRadiusPerLevel, 0f);
            c.orbitBaseTick = Float(v, "OrbitBaseTick", c.orbitBaseTick, 0.01f);
            c.orbitTickPerLevel = Float(v, "OrbitTickPerLevel", c.orbitTickPerLevel, 0f);
            c.orbitMinTick = Float(v, "OrbitMinTick", c.orbitMinTick, 0.01f);
        }

        private static void ApplyCharacters(FarmGameConfig c, IReadOnlyDictionary<string, string> v)
        {
            if (c.characters == null) return;
            for (int i = 0; i < c.characters.Length; i++)
            {
                CharacterDefinition character = c.characters[i];
                if (character == null) continue;
                string prefix = $"Farmer{i}";
                character.damageMultiplier = Float(v, prefix + "DamageMultiplier", character.damageMultiplier, 0.01f);
                character.moveSpeedMultiplier = Float(v, prefix + "MoveSpeedMultiplier", character.moveSpeedMultiplier, 0.01f);
                character.cooldownMultiplier = Float(v, prefix + "CooldownMultiplier", character.cooldownMultiplier, 0.01f);
                character.maxHealthMultiplier = Float(v, prefix + "MaxHealthMultiplier", character.maxHealthMultiplier, 0.01f);
            }
        }

        private static void ApplyEnemies(FarmGameConfig c, IReadOnlyDictionary<string, string> v)
        {
            if (c.enemies == null) return;
            foreach (EnemyDefinition enemy in c.enemies)
            {
                if (enemy == null) continue;
                string prefix = enemy.kind.ToString();
                enemy.maxHealth = Float(v, prefix + "MaxHealth", enemy.maxHealth, 0.1f);
                enemy.moveSpeed = Float(v, prefix + "MoveSpeed", enemy.moveSpeed, 0f);
                enemy.touchDamage = Float(v, prefix + "TouchDamage", enemy.touchDamage, 0f);
                enemy.experience = Int(v, prefix + "Experience", enemy.experience, 0);
                enemy.attackCooldown = Float(v, prefix + "AttackCooldown", enemy.attackCooldown, 0.01f);
            }
        }

        private static float Float(IReadOnlyDictionary<string, string> values, string key, float fallback, float min, float max = float.MaxValue)
        {
            if (!values.TryGetValue(key, out string raw)) return fallback;
            if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                Debug.LogError($"[FarmBalance] '{key}' must be a number, but was '{raw}'. Keeping {fallback}.");
                return fallback;
            }
            return Mathf.Clamp(result, min, max);
        }

        private static int Int(IReadOnlyDictionary<string, string> values, string key, int fallback, int min)
        {
            if (!values.TryGetValue(key, out string raw)) return fallback;
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
            {
                Debug.LogError($"[FarmBalance] '{key}' must be an integer, but was '{raw}'. Keeping {fallback}.");
                return fallback;
            }
            return Mathf.Max(min, result);
        }
    }
}
