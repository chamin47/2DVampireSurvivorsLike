using System;
using TMPro;
using UnityEngine;

namespace DawnFarm
{
    [Serializable]
    public sealed class CharacterDefinition
    {
        public string id;
        public string koreanName;
        public string englishName;
        [TextArea] public string koreanBonus;
        [TextArea] public string englishBonus;
        public Sprite[] standSprites;
        public Sprite[] runSprites;
        public Sprite[] deadSprites;
        public float damageMultiplier = 1f;
        public float moveSpeedMultiplier = 1f;
        public float cooldownMultiplier = 1f;
        public float maxHealthMultiplier = 1f;
    }

    [Serializable]
    public sealed class EnemyDefinition
    {
        public EnemyKind kind;
        public string koreanName;
        public string englishName;
        public Sprite[] runSprites;
        public Sprite hitSprite;
        public Sprite deadSprite;
        public float maxHealth = 10f;
        public float moveSpeed = 1.5f;
        public float touchDamage = 8f;
        public int experience = 1;
        public bool ranged;
        public float attackCooldown = 2.5f;
    }

    [CreateAssetMenu(menuName = "Dawn Farm/Game Config", fileName = "FarmGameConfig")]
    public sealed class FarmGameConfig : ScriptableObject
    {
        [Header("Stage")]
        public float stageDuration = 480f;
        public float baseSpawnInterval = 0.85f;
        public int maxEnemies = 180;
        public float chestInterval = 60f;
        public float finalWaveTime = 450f;
        public int finalWaveCount = 28;
        [Range(0f, 1f)] public float healthDropChance = 0.02f;
        [Range(0f, 1f)] public float magnetDropChance = 0.0075f;
        public float magnetDuration = 6f;
        public float enemyHealthScaleAtEnd = 0.65f;

        [Header("Player")]
        public float baseMaxHealth = 100f;
        public float baseMoveSpeed = 4.2f;
        public float basePickupRadius = 1.8f;
        public CharacterDefinition[] characters;

        [Header("Weapon Balance")]
        public float scytheBaseCooldown = 1.15f;
        public float scytheCooldownPerLevel = 0.07f;
        public float scytheMinCooldown = 0.28f;
        public float scytheBaseDamage = 14f;
        public float scytheDamagePerLevel = 5f;
        public float scytheBaseRadius = 1.65f;
        public float scytheRadiusPerLevel = 0.18f;
        public float seedBaseCooldown = 0.88f;
        public float seedCooldownPerLevel = 0.09f;
        public float seedMinCooldown = 0.18f;
        public float seedBaseDamage = 9f;
        public float seedDamagePerLevel = 3f;
        public float seedProjectileSpeed = 9f;
        public float orbitBaseDamage = 18f;
        public float orbitDamagePerLevel = 2.5f;
        public float orbitBaseSpeed = 105f;
        public float orbitSpeedPerLevel = 14f;
        public float orbitBaseRadius = 1.55f;
        public float orbitRadiusPerLevel = 0.08f;
        public float orbitBaseHitRadius = 0.48f;
        public float orbitHitRadiusPerLevel = 0.04f;
        public float orbitBaseTick = 0.42f;
        public float orbitTickPerLevel = 0.035f;
        public float orbitMinTick = 0.15f;

        [Header("Enemies")]
        public EnemyDefinition[] enemies;

        [Header("World Sprites")]
        public Sprite[] tileSprites;
        public Sprite experienceSprite;
        public Sprite healthSprite;
        public Sprite magnetSprite;
        public Sprite chestSprite;
        public Sprite shadowSprite;
        public Sprite scytheSprite;
        public Sprite seedGunSprite;
        public Sprite pickaxeSprite;
        public Sprite playerBulletSprite;
        public Sprite enemyBulletSprite;

        [Header("UI")]
        public Sprite panelSprite;
        public Sprite titleSprite;
        public TMP_FontAsset font;

        [Header("Audio")]
        public AudioClip bgm;
        public AudioClip melee;
        public AudioClip meleeAlternate;
        public AudioClip ranged;
        public AudioClip hit;
        public AudioClip hitAlternate;
        public AudioClip dead;
        public AudioClip levelUp;
        public AudioClip select;
        public AudioClip win;
        public AudioClip lose;

        public CharacterDefinition GetCharacter(int index)
        {
            if (characters == null || characters.Length == 0) return null;
            return characters[Mathf.Clamp(index, 0, characters.Length - 1)];
        }

        public EnemyDefinition GetEnemy(EnemyKind kind)
        {
            if (enemies == null) return null;
            foreach (var enemy in enemies)
                if (enemy != null && enemy.kind == kind) return enemy;
            return null;
        }
    }
}
