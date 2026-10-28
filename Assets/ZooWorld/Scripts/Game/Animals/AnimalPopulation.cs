using System;
using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Core.Animals;
using ZooWorld.Core.FoodChain;
using ZooWorld.Game.Configs;

namespace ZooWorld.Game.Animals
{
    /// <summary>
    /// The living animals on the scene, kept in sync with the logic registry:
    /// contacts go to the <see cref="CollisionService"/>, deaths decided there
    /// take the matching objects out of play and back to the pool.
    /// </summary>
    public sealed class AnimalPopulation : IAnimalContactListener, IDisposable
    {
        private readonly AnimalRegistry _registry;
        private readonly CollisionService _collisions;
        private readonly AnimalFactory _factory;
        private readonly List<Animal> _animals = new List<Animal>();
        private readonly Dictionary<AnimalId, Animal> _byId = new Dictionary<AnimalId, Animal>();

        public AnimalPopulation(AnimalRegistry registry, CollisionService collisions, AnimalFactory factory)
        {
            _registry = registry;
            _collisions = collisions;
            _factory = factory;

            _registry.Died += OnDied;
            _collisions.Eaten += OnEaten;
        }

        public event Action<Animal> Spawned;

        public IReadOnlyList<Animal> Animals => _animals;

        public bool TryGet(AnimalId id, out Animal animal) => _byId.TryGetValue(id, out animal);

        public Animal Spawn(AnimalConfig config, Vector3 position)
        {
            var animal = _factory.Create(config, position, this);
            _animals.Add(animal);
            _byId.Add(animal.Id, animal);
            Spawned?.Invoke(animal);
            return animal;
        }

        public void OnContact(Animal self, Animal other)
        {
            if (other == null)
            {
                self.Movement.OnBumped(self);
                return;
            }

            // The mirrored callback of a meal: the other one is already gone.
            if (!other.IsAlive) return;

            _collisions.Report(self.Id, other.Id);

            // Still alive and nobody was eaten: physics bounced us, follow the bounce.
            if (self.IsAlive && other.IsAlive) self.Movement.OnBumped(self);
        }

        public void Dispose()
        {
            _registry.Died -= OnDied;
            _collisions.Eaten -= OnEaten;
        }

        private void OnDied(AnimalDeath death)
        {
            if (!_byId.TryGetValue(death.Id, out var animal)) return;

            _byId.Remove(death.Id);
            _animals.Remove(animal);
            animal.Kill(() => _factory.Release(animal));
        }

        private void OnEaten(Meal meal)
        {
            if (_byId.TryGetValue(meal.Eater, out var eater)) eater.NotifyAte();
        }
    }
}
