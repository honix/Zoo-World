using System;
using System.Linq;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using ZooWorld.Core.Animals;
using ZooWorld.Core.FoodChain;
using ZooWorld.Core.Randomness;
using ZooWorld.Core.Spawning;
using ZooWorld.Core.Stats;
using ZooWorld.Core.Time;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Configs;
using ZooWorld.Game.World;

namespace ZooWorld.Game.Infrastructure
{
    /// <summary>
    /// Wires the zoo itself: rules, spawning, animals and the simulation loop.
    /// Shared by the scene's <see cref="ZooLifetimeScope"/> and by the headless tests,
    /// so tests run exactly the wiring the game runs.
    /// </summary>
    public sealed class ZooInstaller : IInstaller
    {
        private readonly WorldSettings _settings;
        private readonly IPlayArea _playArea;

        public ZooInstaller(WorldSettings settings, IPlayArea playArea)
        {
            _settings = settings ? settings : throw new ArgumentNullException(nameof(settings));
            _playArea = playArea ?? throw new ArgumentNullException(nameof(playArea));
        }

        /// <summary>Replaces the seed from the settings, e.g. to replay a run.</summary>
        public int? Seed { get; set; }

        /// <summary>
        /// When true (the game), the simulation steps every FixedUpdate.
        /// Headless runs turn it off and call <see cref="ZooSimulation.Step"/> themselves.
        /// </summary>
        public bool RunInPlayerLoop { get; set; } = true;

        public void Install(IContainerBuilder builder)
        {
            var streams = new RandomStreams(ResolveSeed());
            Debug.Log($"[Zoo World] Master seed: {streams.MasterSeed}");

            builder.RegisterInstance(_settings);
            builder.RegisterInstance(streams);
            builder.RegisterInstance(_playArea);
            builder.Register<ITimeProvider, UnityTimeProvider>(Lifetime.Singleton);

            // Rules (pure C#).
            builder.Register<AnimalRegistry>(Lifetime.Singleton);
            builder.Register<ITieBreaker, CoinFlipTieBreaker>(Lifetime.Singleton).WithParameter(streams.TieBreak);
            builder.Register<IFoodChain, DietFoodChain>(Lifetime.Singleton);
            builder.Register<CollisionService>(Lifetime.Singleton);
            builder.Register<DeathStats>(Lifetime.Singleton);

            // Spawning.
            builder.Register<SpawnTimer>(Lifetime.Singleton)
                .WithParameter(streams.Spawning)
                .WithParameter("minInterval", _settings.MinSpawnInterval)
                .WithParameter("maxInterval", _settings.MaxSpawnInterval);
            builder.RegisterInstance<ISpawnStrategy<AnimalConfig>>(new WeightedRandomSpawnStrategy<AnimalConfig>(
                _settings.SpawnTable
                    .Where(entry => entry.Animal != null)
                    .Select(entry => new SpawnEntry<AnimalConfig>(entry.Animal, entry.Weight)),
                streams.Spawning));
            builder.Register<AnimalSpawner>(Lifetime.Singleton).WithParameter(streams.Spawning);

            // Animals on the scene.
            builder.Register<AnimalProfileMapper>(Lifetime.Singleton);
            builder.Register<AnimalFactory>(Lifetime.Singleton).WithParameter(streams.Movement);
            builder.Register<AnimalPopulation>(Lifetime.Singleton);
            builder.Register<OffScreenSteering>(Lifetime.Singleton).WithParameter(streams.Movement);
            if (RunInPlayerLoop) builder.RegisterEntryPoint<ZooSimulation>().AsSelf();
            else builder.Register<ZooSimulation>(Lifetime.Singleton);
        }

        private int ResolveSeed()
        {
            if (Seed.HasValue) return Seed.Value;
            if (_settings.Seed != 0) return _settings.Seed;
            return Environment.TickCount;
        }
    }
}
