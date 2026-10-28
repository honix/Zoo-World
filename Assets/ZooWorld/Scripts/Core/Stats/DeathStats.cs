using System;
using ZooWorld.Core.Animals;

namespace ZooWorld.Core.Stats
{
    /// <summary>Counts dead prey and dead predators for the UI.</summary>
    public sealed class DeathStats : IDisposable
    {
        private readonly AnimalRegistry _registry;

        public DeathStats(AnimalRegistry registry)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _registry.Died += OnDied;
        }

        public event Action Changed;

        public int DeadPrey { get; private set; }

        public int DeadPredators { get; private set; }

        public int Total => DeadPrey + DeadPredators;

        public void Dispose() => _registry.Died -= OnDied;

        private void OnDied(AnimalDeath death)
        {
            if (death.Profile.Role == AnimalRole.Predator) DeadPredators++;
            else DeadPrey++;

            Changed?.Invoke();
        }
    }
}
