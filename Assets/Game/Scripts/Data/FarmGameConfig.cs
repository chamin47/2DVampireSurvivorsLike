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

        [Header("Player")]
        public float baseMaxHealth = 100f;
        public float baseMoveSpeed = 4.2f;
        public float basePickupRadius = 1.8f;
        public CharacterDefinition[] characters;

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
        public AudioClip ranged;
        public AudioClip hit;
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
