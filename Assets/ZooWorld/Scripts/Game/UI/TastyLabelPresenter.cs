using System;
using UnityEngine;
using UnityEngine.Pool;
using VContainer.Unity;
using ZooWorld.Core.FoodChain;
using ZooWorld.Game.Animals;
using Object = UnityEngine.Object;

namespace ZooWorld.Game.UI
{
    /// <summary>Shows a pooled "Tasty!" label under every predator that eats.</summary>
    public sealed class TastyLabelPresenter : IStartable, IDisposable
    {
        private readonly CollisionService _collisions;
        private readonly AnimalPopulation _population;
        private readonly ObjectPool<TastyLabel> _pool;
        private readonly Transform _root;

        public TastyLabelPresenter(CollisionService collisions, AnimalPopulation population, TastyLabel prefab)
        {
            _collisions = collisions;
            _population = population;
            _root = new GameObject("Tasty Labels").transform;
            _pool = new ObjectPool<TastyLabel>(
                createFunc: () => Object.Instantiate(prefab, _root),
                actionOnGet: label => label.gameObject.SetActive(true),
                actionOnRelease: label => label.gameObject.SetActive(false),
                actionOnDestroy: label => Object.Destroy(label.gameObject),
                collectionCheck: false);
        }

        public void Start() => _collisions.Eaten += OnEaten;

        public void Dispose()
        {
            _collisions.Eaten -= OnEaten;
            _pool.Dispose();
            if (_root != null) Object.Destroy(_root.gameObject);
        }

        private void OnEaten(Meal meal)
        {
            if (!_population.TryGet(meal.Eater, out var eater)) return;

            _pool.Get().Show(eater, _pool.Release);
        }
    }
}
