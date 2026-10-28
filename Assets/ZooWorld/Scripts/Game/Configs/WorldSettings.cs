using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZooWorld.Game.Configs
{
    /// <summary>World tuning for designers: timing, what spawns and how often, play area margins.</summary>
    [CreateAssetMenu(menuName = "Zoo World/World Settings", fileName = "WorldSettings")]
    public sealed class WorldSettings : ScriptableObject
    {
        [Serializable]
        public struct SpawnTableEntry
        {
            public AnimalConfig Animal;

            [Min(0f)]
            [Tooltip("Relative chance. Zero disables the entry.")]
            public float Weight;
        }

        [Header("Randomness")]
        [Tooltip("Master seed for every random stream. 0 picks a new seed each run (it's logged, so a run can be replayed).")]
        [SerializeField] private int _seed;

        [Header("Spawning")]
        [Tooltip("Seconds between spawns, drawn uniformly from [min, max].")]
        [SerializeField, Min(0.01f)] private float _minSpawnInterval = 1f;
        [SerializeField, Min(0.01f)] private float _maxSpawnInterval = 2f;

        [SerializeField] private SpawnTableEntry[] _spawnTable = new SpawnTableEntry[0];

        [Header("Play area")]
        [Tooltip("Animals turn back when they get closer than this to a screen edge, in meters.")]
        [SerializeField, Min(0f)] private float _screenMargin = 0.5f;

        [Tooltip("Animals spawn at least this far inside the screen edges, in meters.")]
        [SerializeField, Min(0f)] private float _spawnMargin = 1.5f;

        public int Seed => _seed;
        public float MinSpawnInterval => _minSpawnInterval;
        public float MaxSpawnInterval => _maxSpawnInterval;
        public IReadOnlyList<SpawnTableEntry> SpawnTable => _spawnTable;
        public float ScreenMargin => _screenMargin;
        public float SpawnMargin => _spawnMargin;

        public IEnumerable<string> Validate()
        {
            if (_minSpawnInterval <= 0f) yield return "Min spawn interval must be positive.";
            if (_maxSpawnInterval < _minSpawnInterval) yield return "Max spawn interval is below min.";
            if (_spawnTable == null || !_spawnTable.Any(entry => entry.Animal != null && entry.Weight > 0f))
                yield return "The spawn table needs at least one animal with a positive weight.";
            if (_spawnTable != null && _spawnTable.Any(entry => entry.Animal == null))
                yield return "The spawn table has an entry without an animal.";
        }
    }
}
