using System;
using System.Collections.Generic;

namespace ZooWorld.Core.Animals
{
    /// <summary>
    /// The set of living animals. An animal is alive exactly while it is registered:
    /// <see cref="Kill"/> removes it at once, so logic never waits for a death animation.
    /// </summary>
    public sealed class AnimalRegistry
    {
        private readonly Dictionary<AnimalId, AnimalProfile> _alive = new Dictionary<AnimalId, AnimalProfile>();
        private int _nextId = 1;

        /// <summary>Raised once per death, right after the animal stops being alive.</summary>
        public event Action<AnimalDeath> Died;

        public int AliveCount => _alive.Count;

        public AnimalId Register(AnimalProfile profile)
        {
            if (profile == null) throw new ArgumentNullException(nameof(profile));

            var id = new AnimalId(_nextId++);
            _alive.Add(id, profile);
            return id;
        }

        public bool IsAlive(AnimalId id) => _alive.ContainsKey(id);

        public bool TryGetProfile(AnimalId id, out AnimalProfile profile) => _alive.TryGetValue(id, out profile);

        /// <summary>Kills a living animal. Returns false if it was already dead or unknown.</summary>
        public bool Kill(AnimalId id)
        {
            if (!_alive.TryGetValue(id, out var profile)) return false;

            _alive.Remove(id);
            Died?.Invoke(new AnimalDeath(id, profile));
            return true;
        }
    }
}
