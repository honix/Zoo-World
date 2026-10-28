using System;
using System.Collections.Generic;
using System.Linq;
using ZooWorld.Core.Randomness;

namespace ZooWorld.Core.Spawning
{
    /// <summary>Picks an entry with probability proportional to its weight.</summary>
    public sealed class WeightedRandomSpawnStrategy<T> : ISpawnStrategy<T>
    {
        private readonly SpawnEntry<T>[] _entries;
        private readonly double _totalWeight;
        private readonly IRandom _random;

        public WeightedRandomSpawnStrategy(IEnumerable<SpawnEntry<T>> entries, IRandom random)
        {
            if (entries == null) throw new ArgumentNullException(nameof(entries));

            _random = random ?? throw new ArgumentNullException(nameof(random));
            _entries = entries.Where(entry => entry.Weight > 0d).ToArray();
            _totalWeight = _entries.Sum(entry => entry.Weight);

            if (_entries.Length == 0)
                throw new ArgumentException("A spawn table needs at least one entry with a positive weight.", nameof(entries));
        }

        public T Next()
        {
            var roll = _random.NextDouble() * _totalWeight;
            foreach (var entry in _entries)
            {
                roll -= entry.Weight;
                if (roll < 0d) return entry.Item;
            }

            // Floating point rounding can leave a tiny remainder: the last entry takes it.
            return _entries[_entries.Length - 1].Item;
        }
    }
}
