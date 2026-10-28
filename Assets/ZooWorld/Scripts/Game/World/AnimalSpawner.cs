using UnityEngine;
using ZooWorld.Core.Randomness;
using ZooWorld.Core.Spawning;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Configs;

namespace ZooWorld.Game.World
{
    /// <summary>When the timer fires, asks the strategy what to spawn and finds a free spot for it.</summary>
    public sealed class AnimalSpawner
    {
        /// <summary>Preferred free distance around a new animal, in meters.</summary>
        private const float Clearance = 1.5f;
        private const int PlacementAttempts = 8;

        private readonly SpawnTimer _timer;
        private readonly ISpawnStrategy<AnimalConfig> _strategy;
        private readonly AnimalPopulation _population;
        private readonly IPlayArea _playArea;
        private readonly IRandom _random;

        public AnimalSpawner(SpawnTimer timer, ISpawnStrategy<AnimalConfig> strategy,
            AnimalPopulation population, IPlayArea playArea, IRandom random)
        {
            _timer = timer;
            _strategy = strategy;
            _population = population;
            _playArea = playArea;
            _random = random;
        }

        public void Tick(float deltaTime)
        {
            for (var due = _timer.Tick(deltaTime); due > 0; due--)
            {
                var config = _strategy.Next();
                _population.Spawn(config, PickPosition());
            }
        }

        /// <summary>A random spot in the spawn area, avoiding other animals when possible.</summary>
        private Vector3 PickPosition()
        {
            var area = _playArea.SpawnBounds;
            var position = area.RandomPoint(_random);

            for (var attempt = 1; attempt < PlacementAttempts && IsCrowded(position); attempt++)
            {
                position = area.RandomPoint(_random);
            }

            return position;
        }

        private bool IsCrowded(Vector3 position)
        {
            foreach (var animal in _population.Animals)
            {
                if ((animal.Position - position).sqrMagnitude < Clearance * Clearance) return true;
            }

            return false;
        }
    }
}
