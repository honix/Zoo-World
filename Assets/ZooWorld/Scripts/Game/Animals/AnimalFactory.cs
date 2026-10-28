using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using ZooWorld.Core.Animals;
using ZooWorld.Core.Randomness;
using ZooWorld.Game.Configs;
using Object = UnityEngine.Object;

namespace ZooWorld.Game.Animals
{
    /// <summary>
    /// Creates animals from configs: takes an object from the prefab's pool,
    /// registers the animal in logic and gives it a fresh movement.
    /// </summary>
    public sealed class AnimalFactory : IDisposable
    {
        private readonly AnimalRegistry _registry;
        private readonly AnimalProfileMapper _profiles;
        private readonly IRandom _movementRandom;
        private readonly Transform _root;
        private readonly Dictionary<Animal, ObjectPool<Animal>> _poolsByPrefab = new Dictionary<Animal, ObjectPool<Animal>>();
        private readonly Dictionary<Animal, ObjectPool<Animal>> _poolsByInstance = new Dictionary<Animal, ObjectPool<Animal>>();

        public AnimalFactory(AnimalRegistry registry, AnimalProfileMapper profiles, IRandom movementRandom)
        {
            _registry = registry;
            _profiles = profiles;
            _movementRandom = movementRandom;
            _root = new GameObject("Animals").transform;
        }

        public Animal Create(AnimalConfig config, Vector3 position, IAnimalContactListener contacts)
        {
            var animal = PoolFor(config.Prefab).Get();
            var id = _registry.Register(_profiles.GetProfile(config));
            animal.Activate(id, config, config.Movement.CreateMovement(_movementRandom), contacts, position);
            return animal;
        }

        public void Release(Animal animal)
        {
            if (_poolsByInstance.TryGetValue(animal, out var pool)) pool.Release(animal);
        }

        public void Dispose()
        {
            foreach (var pool in _poolsByPrefab.Values) pool.Dispose();
            _poolsByPrefab.Clear();
            _poolsByInstance.Clear();
            if (_root != null) Destroy(_root.gameObject);
        }

        private ObjectPool<Animal> PoolFor(Animal prefab)
        {
            if (_poolsByPrefab.TryGetValue(prefab, out var pool)) return pool;

            pool = null;
            pool = new ObjectPool<Animal>(
                createFunc: () =>
                {
                    var instance = Object.Instantiate(prefab, _root);
                    instance.name = prefab.name;
                    _poolsByInstance.Add(instance, pool);
                    return instance;
                },
                actionOnGet: animal => animal.gameObject.SetActive(true),
                actionOnRelease: animal => animal.gameObject.SetActive(false),
                actionOnDestroy: animal =>
                {
                    _poolsByInstance.Remove(animal);
                    if (animal != null) Destroy(animal.gameObject);
                },
                collectionCheck: false);

            _poolsByPrefab.Add(prefab, pool);
            return pool;
        }

        private static void Destroy(Object target)
        {
            if (Application.isPlaying) Object.Destroy(target);
            else Object.DestroyImmediate(target);
        }
    }
}
