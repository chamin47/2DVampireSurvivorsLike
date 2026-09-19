using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Framework.Asset;
using Framework.Audio;
using Framework.Event;
using Framework.Scene.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace DawnFarm
{
    public sealed class FarmGameBootstrap : MonoBehaviour, IFarmGameSession
    {
        public const string PlayerAddress = "farm/player";
        public const string EnemyAddress = "farm/enemy";
        public const string ProjectileAddress = "farm/projectile";
        public const string PickupAddress = "farm/pickup";
        public const string WeaponVisualAddress = "farm/weapon-visual";

        [SerializeField] private FarmGameConfig config;
        [SerializeField] private Camera gameplayCamera;

        private readonly List<FarmEnemy> enemies = new List<FarmEnemy>();
        private Transform runtimeRoot;
        private FarmGameUI ui;
        private FarmLocalization localization;
        private FarmEffectService effects;
        private FarmWeaponSystem weapons;
        private FarmPlayer player;
        private FarmGameState state = FarmGameState.Title;
        private int selectedCharacter;
        private int level = 1;
        private int experience;
        private int requiredExperience = 5;
        private int killCount;
        private float elapsedTime;
        private float spawnTimer;
        private float chestTimer;
        private float magnetUntil;
        private bool bossSpawned;
        private bool finalWaveSpawned;

        public FarmGameConfig Config => config;
        public FarmGameState State => state;
        public FarmPlayer Player => player;
        public IReadOnlyList<FarmEnemy> Enemies => enemies;
        public Transform RuntimeRoot => runtimeRoot;
        public float ElapsedTime => elapsedTime;
        public int KillCount => killCount;
        public bool IsMagnetActive => Time.unscaledTime < magnetUntil;

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError("[DawnFarm] FarmGameConfig is not assigned. Run Tools/Dawn Farm/Build Complete Game.");
                enabled = false;
                return;
            }
            RegisterAddressableDefinitions();
            GameServices.Clear();
            GameServices.Register<IFarmGameSession>(this);
            localization = new FarmLocalization();
            GameServices.Register(localization);
            runtimeRoot = new GameObject("[Runtime Game Objects]").transform;
            runtimeRoot.SetParent(transform, false);
            effects = new GameObject("Effects").AddComponent<FarmEffectService>();
            effects.transform.SetParent(runtimeRoot, false);
            GameServices.Register(effects);
            ui = new GameObject("UI System").AddComponent<FarmGameUI>();
            ui.transform.SetParent(transform, false);
            ui.Build(config, localization, SelectCharacter, StartGame, RestartGame, ChooseUpgrade);
            if (gameplayCamera == null) gameplayCamera = Camera.main;
            if (gameplayCamera != null && gameplayCamera.GetComponent<FarmCameraFollow>() == null) gameplayCamera.gameObject.AddComponent<FarmCameraFollow>();
            UIManagerWarmup();
        }

        private void Start()
        {
            Time.timeScale = 1f;
            state = FarmGameState.Title;
            ui.ShowTitle(selectedCharacter);
            if (config.bgm != null) AudioPlayer.PlayBgm(config.bgm, true, 0.8f, 0.52f);
        }

        private static void UIManagerWarmup()
        {
            _ = Framework.UI.UIManager.Instance;
        }

        private void RegisterAddressableDefinitions()
        {
            AssetLoader.RegisterDefinitions(new[]
            {
                new AssetDefinition(PlayerAddress, PlayerAddress, AssetSource.Addressables),
                new AssetDefinition(EnemyAddress, EnemyAddress, AssetSource.Addressables),
                new AssetDefinition(ProjectileAddress, ProjectileAddress, AssetSource.Addressables),
                new AssetDefinition(PickupAddress, PickupAddress, AssetSource.Addressables),
                new AssetDefinition(WeaponVisualAddress, WeaponVisualAddress, AssetSource.Addressables),
            });
        }

        private void Update()
        {
            if (state != FarmGameState.Playing) return;
            elapsedTime += Time.deltaTime;
            spawnTimer -= Time.deltaTime;
            chestTimer -= Time.deltaTime;
            EventBus.Publish(new RunStatsChangedEvent(elapsedTime, killCount));

            if (elapsedTime >= config.stageDuration)
            {
                if (!bossSpawned) SpawnBoss();
            }
            else
            {
                if (spawnTimer <= 0f && enemies.Count < config.maxEnemies)
                {
                    SpawnScheduledEnemy();
                    float progress = Mathf.Clamp01(elapsedTime / config.stageDuration);
                    spawnTimer = Mathf.Lerp(config.baseSpawnInterval, 0.16f, progress);
                }
                if (!finalWaveSpawned && elapsedTime >= 450f)
                {
                    finalWaveSpawned = true;
                    for (int i = 0; i < 28; i++) SpawnEnemy(ChooseEnemyKind(449f), RandomSpawnPosition(8f, 12f));
                }
            }

            if (chestTimer <= 0f)
            {
                chestTimer = config.chestInterval;
                SpawnPickup(PickupKind.Chest, 1, RandomSpawnPosition(4f, 8f));
            }

            CheckDebugKeys();
        }

        private void SelectCharacter(int index)
        {
            selectedCharacter = Mathf.Clamp(index, 0, Mathf.Max(0, config.characters.Length - 1));
            if (config.select != null) AudioPlayer.PlaySfx(config.select, 0.6f);
        }

        private void StartGame()
        {
            if (state != FarmGameState.Title) return;
            Time.timeScale = 1f;
            elapsedTime = 0f;
            killCount = 0;
            level = 1;
            experience = 0;
            requiredExperience = 5;
            spawnTimer = 0.3f;
            chestTimer = config.chestInterval;
            bossSpawned = false;
            finalWaveSpawned = false;
            enemies.Clear();

            var playerObject = AssetLoader.Spawn(PlayerAddress, runtimeRoot, AssetSource.Addressables);
            if (playerObject == null)
            {
                Debug.LogError("[DawnFarm] Addressable player prefab could not be spawned.");
                return;
            }
            playerObject.name = "Player";
            playerObject.transform.position = Vector3.zero;
            player = playerObject.GetComponent<FarmPlayer>();
            player.Configure(this, config.GetCharacter(selectedCharacter));
            weapons = playerObject.GetComponent<FarmWeaponSystem>();
            weapons.Configure(this, player);
            if (gameplayCamera != null)
            {
                var follow = gameplayCamera.GetComponent<FarmCameraFollow>();
                if (follow == null) follow = gameplayCamera.gameObject.AddComponent<FarmCameraFollow>();
                follow.Target = player.transform;
                gameplayCamera.transform.position = new Vector3(0f, 0f, -10f);
            }
            var tileRoot = new GameObject("Infinite Farm Floor");
            tileRoot.transform.SetParent(runtimeRoot, false);
            tileRoot.AddComponent<FarmWorldTiler>().Configure(player.transform, config.tileSprites, 27, 17);
            state = FarmGameState.Playing;
            ui.ShowPlaying();
            EventBus.Publish(new ExperienceChangedEvent(level, experience, requiredExperience));
            EventBus.Publish(new RunStatsChangedEvent(elapsedTime, killCount));
            Debug.Log("[DawnFarm] Run started. Addressable prefabs, pooling, EventBus, audio, localization and data config are active.");
        }

        private void SpawnScheduledEnemy()
        {
            int batch = elapsedTime >= 360f ? 2 : 1;
            for (int i = 0; i < batch && enemies.Count < config.maxEnemies; i++)
                SpawnEnemy(ChooseEnemyKind(elapsedTime), RandomSpawnPosition(7f, 10f));
        }

        private EnemyKind ChooseEnemyKind(float time)
        {
            float roll = Random.value;
            if (time < 120f) return EnemyKind.Zombie;
            if (time < 240f) return roll < 0.38f ? EnemyKind.Runner : EnemyKind.Zombie;
            if (time < 360f)
            {
                if (roll < 0.22f) return EnemyKind.Mummy;
                if (roll < 0.52f) return EnemyKind.Skeleton;
                if (roll < 0.72f) return EnemyKind.Runner;
                return EnemyKind.Zombie;
            }
            if (roll < 0.28f) return EnemyKind.Mummy;
            if (roll < 0.54f) return EnemyKind.Skeleton;
            if (roll < 0.76f) return EnemyKind.Runner;
            return EnemyKind.Zombie;
        }

        private Vector3 RandomSpawnPosition(float minRadius, float maxRadius)
        {
            Vector2 center = player != null ? player.transform.position : Vector2.zero;
            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction == Vector2.zero) direction = Vector2.right;
            return center + direction * Random.Range(minRadius, maxRadius);
        }

        private FarmEnemy SpawnEnemy(EnemyKind kind, Vector3 position)
        {
            var definition = config.GetEnemy(kind);
            if (definition == null) return null;
            var enemyObject = AssetLoader.Spawn(EnemyAddress, runtimeRoot, AssetSource.Addressables);
            if (enemyObject == null) return null;
            enemyObject.name = kind.ToString();
            var enemy = enemyObject.GetComponent<FarmEnemy>();
            float healthScale = 1f + Mathf.Clamp01(elapsedTime / config.stageDuration) * 0.65f;
            enemy.Configure(this, definition, position, healthScale);
            return enemy;
        }

        private void SpawnBoss()
        {
            bossSpawned = true;
            for (int i = enemies.Count - 1; i >= 0; i--)
                if (enemies[i] != null && enemies[i].Kind != EnemyKind.Reaper) AssetLoader.Despawn(enemies[i].gameObject);
            enemies.Clear();
            SpawnEnemy(EnemyKind.Reaper, RandomSpawnPosition(8f, 9f));
            Debug.Log("[DawnFarm] Eight minutes reached. Normal spawning stopped and the Reaper boss appeared.");
        }

        public FarmEnemy FindNearestEnemy(Vector3 position)
        {
            FarmEnemy result = null;
            float best = float.MaxValue;
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;
                float distance = (enemy.transform.position - position).sqrMagnitude;
                if (distance < best) { best = distance; result = enemy; }
            }
            return result;
        }

        public void RegisterEnemy(FarmEnemy enemy)
        {
            if (enemy != null && !enemies.Contains(enemy)) enemies.Add(enemy);
        }

        public void UnregisterEnemy(FarmEnemy enemy) => enemies.Remove(enemy);

        public void EnemyDefeated(FarmEnemy enemy)
        {
            killCount++;
            var definition = config.GetEnemy(enemy.Kind);
            int xp = definition != null ? definition.experience : 1;
            SpawnPickup(PickupKind.Experience, xp, enemy.transform.position);
            if (Random.value < 0.02f) SpawnPickup(PickupKind.Health, 20, enemy.transform.position + (Vector3)Random.insideUnitCircle * 0.3f);
            if (Random.value < 0.0075f) SpawnPickup(PickupKind.Magnet, 1, enemy.transform.position + (Vector3)Random.insideUnitCircle * 0.3f);
            EventBus.Publish(new RunStatsChangedEvent(elapsedTime, killCount));
            if (enemy.Kind == EnemyKind.Reaper) StartCoroutine(WinAfterDelay());
        }

        private IEnumerator WinAfterDelay()
        {
            yield return new WaitForSecondsRealtime(0.55f);
            EndGame(true);
        }

        private FarmPickup SpawnPickup(PickupKind kind, int amount, Vector3 position)
        {
            var pickupObject = AssetLoader.Spawn(PickupAddress, runtimeRoot, AssetSource.Addressables);
            if (pickupObject == null) return null;
            Sprite sprite = kind == PickupKind.Experience ? config.experienceSprite : kind == PickupKind.Health ? config.healthSprite : kind == PickupKind.Magnet ? config.magnetSprite : config.chestSprite;
            var pickup = pickupObject.GetComponent<FarmPickup>();
            pickup.Configure(this, kind, amount, position, sprite);
            return pickup;
        }

        public void AddExperience(int amount)
        {
            if (state != FarmGameState.Playing) return;
            experience += Mathf.Max(0, amount);
            EventBus.Publish(new ExperienceChangedEvent(level, experience, requiredExperience));
            if (experience < requiredExperience) return;
            experience -= requiredExperience;
            level++;
            requiredExperience = Mathf.RoundToInt(5f + level * 2.8f + level * level * 0.22f);
            state = FarmGameState.LevelUp;
            Time.timeScale = 0f;
            if (config.levelUp != null) AudioPlayer.PlaySfx(config.levelUp, 0.75f);
            ui.ShowLevelUp(weapons.CreateChoices());
            EventBus.Publish(new ExperienceChangedEvent(level, experience, requiredExperience));
        }

        public void CollectPickup(PickupKind kind, int amount)
        {
            switch (kind)
            {
                case PickupKind.Experience: AddExperience(amount); break;
                case PickupKind.Health: player?.Heal(amount); break;
                case PickupKind.Magnet: magnetUntil = Time.unscaledTime + 6f; break;
                case PickupKind.Chest:
                    weapons?.UpgradeRandomOwnedWeapon();
                    effects?.Burst(player.transform.position, new Color(1f, 0.75f, 0.18f), 12);
                    break;
            }
            if (config.select != null && kind != PickupKind.Experience) AudioPlayer.PlaySfx(config.select, 0.55f, 1.08f);
        }

        private void ChooseUpgrade(UpgradeKind kind)
        {
            if (state != FarmGameState.LevelUp) return;
            weapons.ApplyUpgrade(kind);
            ui.HideLevelUp();
            state = FarmGameState.Playing;
            Time.timeScale = 1f;
            if (experience >= requiredExperience) AddExperience(0);
        }

        public FarmProjectile SpawnProjectile(Vector3 position, Vector2 direction, bool hostile, float damage, float speed, int penetration, Sprite sprite)
        {
            var projectileObject = AssetLoader.Spawn(ProjectileAddress, runtimeRoot, AssetSource.Addressables);
            if (projectileObject == null) return null;
            var projectile = projectileObject.GetComponent<FarmProjectile>();
            projectile.Configure(this, position, direction, hostile, damage, speed, penetration, sprite);
            return projectile;
        }

        public GameObject SpawnWeaponVisual(Vector3 position, Sprite sprite)
        {
            var visual = AssetLoader.Spawn(WeaponVisualAddress, runtimeRoot, AssetSource.Addressables);
            if (visual == null) return null;
            visual.transform.position = position;
            visual.GetComponent<SpriteRenderer>().sprite = sprite;
            return visual;
        }

        public void EndGame(bool won)
        {
            if (state == FarmGameState.Won || state == FarmGameState.Lost) return;
            state = won ? FarmGameState.Won : FarmGameState.Lost;
            Time.timeScale = 0f;
            if (won && config.win != null) AudioPlayer.PlaySfx(config.win, 0.9f);
            if (!won && config.lose != null) AudioPlayer.PlaySfx(config.lose, 0.9f);
            ui.ShowResult(won, elapsedTime, level, killCount);
        }

        private async void RestartGame()
        {
            Time.timeScale = 1f;
            bool loaded = await GameSceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
            if (!loaded) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void CheckDebugKeys()
        {
#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.f1Key.wasPressedThisFrame) AddExperience(requiredExperience);
            if (keyboard.f2Key.wasPressedThisFrame && !bossSpawned) { elapsedTime = config.stageDuration; SpawnBoss(); }
            if (keyboard.f3Key.wasPressedThisFrame) player?.Heal(999f);
#endif
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f;
            GameServices.Clear();
        }
    }
}
