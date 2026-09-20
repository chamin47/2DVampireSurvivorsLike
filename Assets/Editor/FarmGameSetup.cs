using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DawnFarm;
using Framework.Asset;
using Framework.Audio;
using Framework.UI;
using TMPro;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace DawnFarm.Editor
{
    public static class FarmGameSetup
    {
        private const string Root = "Assets";
        private const string DataPath = Root + "/Data/FarmGameConfig.asset";
        private const string FontPath = Root + "/UI/NeoDGM Dynamic.asset";
        private const string ScenePath = Root + "/Scenes/FarmSurvivors.unity";
        private const string PrefabRoot = Root + "/Prefabs";

        [MenuItem("Tools/Dawn Farm/Build Complete Game")]
        public static void BuildAll()
        {
            EnsureFolders();
            AssetDatabase.Refresh();
            FarmGameConfig config = CreateOrUpdateConfig();
            CreateGameplayPrefabs(config);
            RegisterAddressables();
            CreateGameScene(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[DawnFarmSetup] Complete game created. Open Assets/Scenes/FarmSurvivors.unity and press Play.");
        }

        private static void EnsureFolders()
        {
            foreach (string folder in new[] { Root, Root + "/Data", Root + "/Prefabs", Root + "/Scenes", Root + "/UI" })
                Directory.CreateDirectory(folder);
        }

        private static FarmGameConfig CreateOrUpdateConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<FarmGameConfig>(DataPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<FarmGameConfig>();
                AssetDatabase.CreateAsset(config, DataPath);
            }

            config.stageDuration = 480f;
            config.baseSpawnInterval = 0.85f;
            config.maxEnemies = 180;
            config.chestInterval = 60f;
            config.baseMaxHealth = 100f;
            config.baseMoveSpeed = 4.2f;
            config.basePickupRadius = 1.8f;
            config.font = CreateOrLoadFont();
            config.characters = new[]
            {
                Character(0, "균형 농부", "BALANCED", "공격력 +10%", "Damage +10%", 1.10f, 1f, 1f, 1f),
                Character(1, "바람 농부", "SPRINTER", "이동속도 +15%", "Move speed +15%", 1f, 1.15f, 1f, 1f),
                Character(2, "속사 농부", "QUICKSHOT", "쿨타임 -10%", "Cooldown -10%", 1f, 1f, 0.90f, 1f),
                Character(3, "강철 농부", "SURVIVOR", "최대 체력 +25%", "Max health +25%", 1f, 1f, 1f, 1.25f),
            };
            config.enemies = new[]
            {
                Enemy(0, EnemyKind.Zombie, "좀비", "ZOMBIE", 18f, 1.45f, 8f, 1, false, 2.5f),
                Enemy(1, EnemyKind.Runner, "빠른 좀비", "RUNNER", 14f, 2.45f, 7f, 2, false, 2.2f),
                Enemy(2, EnemyKind.Skeleton, "해골", "SKELETON", 48f, 1.05f, 13f, 4, false, 2.5f),
                Enemy(3, EnemyKind.Mummy, "미라", "MUMMY", 34f, 1.20f, 10f, 4, true, 2.35f),
                Enemy(4, EnemyKind.Reaper, "사신", "REAPER", 950f, 1.15f, 22f, 80, false, 1.8f),
            };

            config.tileSprites = LoadSprites("Assets/Undead Survivor/Sprites/Tiles.png").Where(s => s.name.StartsWith("Tile ")).ToArray();
            config.experienceSprite = LoadSprite("Assets/Undead Survivor/Sprites/Props.png", "Exp 0");
            config.healthSprite = LoadSprite("Assets/Undead Survivor/Sprites/Props.png", "Health");
            config.magnetSprite = LoadSprite("Assets/Undead Survivor/Sprites/Props.png", "Mag");
            config.chestSprite = LoadSprite("Assets/Undead Survivor/Sprites/Props.png", "Box");
            config.shadowSprite = LoadSprite("Assets/Undead Survivor/Sprites/Props.png", "Shadow");
            config.scytheSprite = LoadSprite("Assets/Undead Survivor/Sprites/Props.png", "Weapon 0");
            config.seedGunSprite = LoadSprite("Assets/Undead Survivor/Sprites/Props.png", "Weapon 0");
            config.pickaxeSprite = LoadSprite("Assets/Undead Survivor/Sprites/Props.png", "Weapon 2");
            config.playerBulletSprite = LoadSprite("Assets/Undead Survivor/Sprites/Props.png", "Bullet 0");
            config.enemyBulletSprite = LoadSprite("Assets/Undead Survivor/Sprites/Props.png", "Bullet 5");
            config.panelSprite = LoadSprite("Assets/Undead Survivor/Sprites/UI.png", "Panel");
            config.titleSprite = LoadSprite("Assets/Undead Survivor/Sprites/UI.png", "Title 0");

            config.bgm = LoadAudio("BGM.wav");
            config.melee = LoadAudio("Melee0.wav");
            config.meleeAlternate = LoadAudio("Melee1.wav");
            config.ranged = LoadAudio("Range.wav");
            config.hit = LoadAudio("Hit0.wav");
            config.hitAlternate = LoadAudio("Hit1.wav");
            config.dead = LoadAudio("Dead.wav");
            config.levelUp = LoadAudio("LevelUp.wav");
            config.select = LoadAudio("Select.wav");
            config.win = LoadAudio("Win.wav");
            config.lose = LoadAudio("Lose.wav");
            EditorUtility.SetDirty(config);
            return config;
        }

        private static CharacterDefinition Character(int index, string ko, string en, string koBonus, string enBonus, float damage, float speed, float cooldown, float health)
        {
            var sprites = LoadSprites($"Assets/Undead Survivor/Sprites/Farmer {index}.png");
            return new CharacterDefinition
            {
                id = $"farmer_{index}", koreanName = ko, englishName = en, koreanBonus = koBonus, englishBonus = enBonus,
                standSprites = sprites.Where(s => s.name.StartsWith("Stand ")).OrderBy(s => s.name).ToArray(),
                runSprites = sprites.Where(s => s.name.StartsWith("Run ")).OrderBy(s => s.name).ToArray(),
                deadSprites = sprites.Where(s => s.name.StartsWith("Dead ")).OrderBy(s => s.name).ToArray(),
                damageMultiplier = damage, moveSpeedMultiplier = speed, cooldownMultiplier = cooldown, maxHealthMultiplier = health,
            };
        }

        private static EnemyDefinition Enemy(int index, EnemyKind kind, string ko, string en, float hp, float speed, float damage, int xp, bool ranged, float cooldown)
        {
            var sprites = LoadSprites($"Assets/Undead Survivor/Sprites/Enemy {index}.png");
            return new EnemyDefinition
            {
                kind = kind, koreanName = ko, englishName = en,
                runSprites = sprites.Where(s => s.name.StartsWith("Run ")).OrderBy(s => s.name).ToArray(),
                hitSprite = sprites.FirstOrDefault(s => s.name == "Hit"),
                deadSprite = sprites.FirstOrDefault(s => s.name == "Dead"),
                maxHealth = hp, moveSpeed = speed, touchDamage = damage, experience = xp, ranged = ranged, attackCooldown = cooldown,
            };
        }

        private static TMP_FontAsset CreateOrLoadFont()
        {
            var asset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (asset != null && asset.atlasTextures != null && asset.atlasTextures.Length > 0 && asset.atlasTextures[0] != null) return asset;
            if (asset != null) AssetDatabase.DeleteAsset(FontPath);
            var source = AssetDatabase.LoadAssetAtPath<Font>("Assets/Undead Survivor/Fonts/neodgm.ttf");
            if (source == null) return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            asset = TMP_FontAsset.CreateFontAsset(source);
            asset.name = "NeoDGM Dynamic";
            asset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            AssetDatabase.CreateAsset(asset, FontPath);
            if (asset.material != null)
            {
                asset.material.name = "NeoDGM Dynamic Material";
                AssetDatabase.AddObjectToAsset(asset.material, asset);
            }
            if (asset.atlasTextures != null)
            {
                for (int i = 0; i < asset.atlasTextures.Length; i++)
                {
                    if (asset.atlasTextures[i] == null) continue;
                    asset.atlasTextures[i].name = $"NeoDGM Dynamic Atlas {i}";
                    AssetDatabase.AddObjectToAsset(asset.atlasTextures[i], asset);
                }
            }
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        private static void CreateGameplayPrefabs(FarmGameConfig config)
        {
            CreatePrefab("Player", go =>
            {
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 20;
                renderer.sprite = config.characters[0].standSprites.FirstOrDefault();
                go.AddComponent<SpriteFlipbook>();
                go.AddComponent<FarmPlayer>();
                go.AddComponent<FarmWeaponSystem>();
            });
            CreatePrefab("Enemy", go =>
            {
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 10;
                renderer.sprite = config.enemies[0].runSprites.FirstOrDefault();
                go.AddComponent<SpriteFlipbook>();
                go.AddComponent<FarmEnemy>();
            });
            CreatePrefab("Projectile", go =>
            {
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 30;
                renderer.sprite = config.playerBulletSprite;
                go.AddComponent<FarmProjectile>();
            });
            CreatePrefab("Pickup", go =>
            {
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 8;
                renderer.sprite = config.experienceSprite;
                go.AddComponent<FarmPickup>();
            });
            CreatePrefab("WeaponVisual", go =>
            {
                var renderer = go.AddComponent<SpriteRenderer>();
                renderer.sortingOrder = 25;
                renderer.sprite = config.pickaxeSprite;
                go.AddComponent<TimedWeaponVisual>();
            });
        }

        private static void CreatePrefab(string name, Action<GameObject> configure)
        {
            string path = $"{PrefabRoot}/{name}.prefab";
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) AssetDatabase.DeleteAsset(path);
            GameObject root = new GameObject(name);
            configure(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
        }

        private static void RegisterAddressables()
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) throw new InvalidOperationException("AddressableAssetSettings is missing.");
            var addresses = new Dictionary<string, string>
            {
                [$"{PrefabRoot}/Player.prefab"] = FarmGameBootstrap.PlayerAddress,
                [$"{PrefabRoot}/Enemy.prefab"] = FarmGameBootstrap.EnemyAddress,
                [$"{PrefabRoot}/Projectile.prefab"] = FarmGameBootstrap.ProjectileAddress,
                [$"{PrefabRoot}/Pickup.prefab"] = FarmGameBootstrap.PickupAddress,
                [$"{PrefabRoot}/WeaponVisual.prefab"] = FarmGameBootstrap.WeaponVisualAddress,
            };
            foreach (var pair in addresses)
            {
                string guid = AssetDatabase.AssetPathToGUID(pair.Key);
                var entry = settings.FindAssetEntry(guid) ?? settings.CreateOrMoveEntry(guid, settings.DefaultGroup, false, false);
                entry.address = pair.Value;
            }
            EditorUtility.SetDirty(settings);
        }

        private static void CreateGameScene(FarmGameConfig config)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "FarmSurvivors";
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(FarmCameraFollow));
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.8f;
            camera.backgroundColor = new Color(0.035f, 0.055f, 0.06f, 1f);
            camera.transform.position = new Vector3(0f, 0f, -10f);

            var framework = new GameObject("[Framework Services]");
            framework.AddComponent<AssetLoader>();
            framework.AddComponent<AudioPlayer>();
            framework.AddComponent<UIManager>();

            var bootstrapObject = new GameObject("GameBootstrap");
            var bootstrap = bootstrapObject.AddComponent<FarmGameBootstrap>();
            var serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("config").objectReferenceValue = config;
            serialized.FindProperty("gameplayCamera").objectReferenceValue = camera;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetAsLastSibling();

            EditorSceneManager.SaveScene(scene, ScenePath);
            var scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.All(item => item.path != ScenePath)) scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            else scenes = scenes.Select(item => new EditorBuildSettingsScene(item.path, item.path == ScenePath || item.enabled)).ToList();
            EditorBuildSettings.scenes = scenes.ToArray();
            Selection.activeGameObject = bootstrapObject;
        }

        private static Sprite[] LoadSprites(string path) => AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToArray();
        private static Sprite LoadSprite(string path, string name) => LoadSprites(path).FirstOrDefault(sprite => sprite.name == name);
        private static AudioClip LoadAudio(string fileName) => AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Undead Survivor/Audio/{fileName}");
    }
}
