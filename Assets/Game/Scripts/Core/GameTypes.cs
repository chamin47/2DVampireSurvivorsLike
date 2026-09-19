using System;
using System.Collections.Generic;
using UnityEngine;

namespace DawnFarm
{
    public enum FarmGameState { Title, Playing, LevelUp, Won, Lost }
    public enum EnemyKind { Zombie, Runner, Skeleton, Mummy, Reaper }
    public enum PickupKind { Experience, Health, Magnet, Chest }
    public enum WeaponKind { Scythe, SeedGun, OrbitingPickaxe }
    public enum UpgradeKind { Scythe, SeedGun, OrbitingPickaxe, Power, MoveSpeed, Cooldown, PickupRadius, MaxHealth }

    public readonly struct HealthChangedEvent
    {
        public readonly float Current;
        public readonly float Maximum;
        public HealthChangedEvent(float current, float maximum) { Current = current; Maximum = maximum; }
    }

    public readonly struct ExperienceChangedEvent
    {
        public readonly int Level;
        public readonly int Current;
        public readonly int Required;
        public ExperienceChangedEvent(int level, int current, int required) { Level = level; Current = current; Required = required; }
    }

    public readonly struct RunStatsChangedEvent
    {
        public readonly float Elapsed;
        public readonly int Kills;
        public RunStatsChangedEvent(float elapsed, int kills) { Elapsed = elapsed; Kills = kills; }
    }

    public readonly struct LanguageChangedEvent { }

    public interface IFarmGameSession
    {
        FarmGameConfig Config { get; }
        FarmGameState State { get; }
        FarmPlayer Player { get; }
        IReadOnlyList<FarmEnemy> Enemies { get; }
        Transform RuntimeRoot { get; }
        float ElapsedTime { get; }
        int KillCount { get; }
        bool IsMagnetActive { get; }
        FarmEnemy FindNearestEnemy(Vector3 position);
        void RegisterEnemy(FarmEnemy enemy);
        void UnregisterEnemy(FarmEnemy enemy);
        void EnemyDefeated(FarmEnemy enemy);
        void AddExperience(int amount);
        void CollectPickup(PickupKind kind, int amount);
        FarmProjectile SpawnProjectile(Vector3 position, Vector2 direction, bool hostile, float damage, float speed, int penetration, Sprite sprite);
        GameObject SpawnWeaponVisual(Vector3 position, Sprite sprite);
        void EndGame(bool won);
    }

    public static class GameServices
    {
        private static readonly Dictionary<Type, object> Services = new Dictionary<Type, object>();

        public static void Register<T>(T service) where T : class => Services[typeof(T)] = service;
        public static T Get<T>() where T : class => Services.TryGetValue(typeof(T), out var value) ? value as T : null;
        public static void Clear() => Services.Clear();
    }
}
