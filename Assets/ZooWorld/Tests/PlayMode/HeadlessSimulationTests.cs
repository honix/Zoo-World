using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using ZooWorld.Core.Animals;
using ZooWorld.Core.FoodChain;
using ZooWorld.Core.Stats;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Configs;
using ZooWorld.Game.Infrastructure;
using ZooWorld.Game.World;

namespace ZooWorld.Game.PlayModeTests
{
    /// <summary>
    /// Runs the real zoo wiring without a camera or UI, with a fixed seed and
    /// manually stepped physics, so minutes of zoo life take a moment.
    /// </summary>
    public class HeadlessSimulationTests
    {
        private const float Step = 0.02f;
        private const int StepsPerFrame = 250;

        /// <summary>Longest an animal may stay outside the visible area, in seconds.</summary>
        private const float MaxTimeOffScreen = 3f;

        private static readonly GroundRect Visible = new GroundRect(-14f, -8f, 14f, 8f);

        private readonly List<IObjectResolver> _containers = new List<IObjectResolver>();
        private SimulationMode _previousSimulationMode;

        [SetUp]
        public void SetUp()
        {
            _previousSimulationMode = Physics.simulationMode;
            Physics.simulationMode = SimulationMode.Script;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var container in _containers) container.Dispose();
            _containers.Clear();
            Physics.simulationMode = _previousSimulationMode;
        }

        [UnityTest]
        public IEnumerator TwoMinutesOfZooLife_KeepInvariants()
        {
            var run = new Run(Build(seed: 2026));
            var offScreenSince = new Dictionary<AnimalId, float>();

            yield return run.Simulate(seconds: 120f, afterStep: time =>
            {
                foreach (var animal in run.Population.Animals)
                {
                    var position = animal.Position;
                    Assert.That(float.IsNaN(position.x) || float.IsNaN(position.z), Is.False, $"{animal.name} has a NaN position");
                    Assert.That(Visible.Inset(-10f).Contains(position), Is.True, $"{animal.name} ran away to {position}");

                    if (Visible.Contains(position)) offScreenSince.Remove(animal.Id);
                    else if (!offScreenSince.ContainsKey(animal.Id)) offScreenSince[animal.Id] = time;
                    else Assert.That(time - offScreenSince[animal.Id], Is.LessThan(MaxTimeOffScreen), $"{animal.name} stays off screen");
                }
            });

            Assert.That(run.Spawned, Is.GreaterThan(60), "about one spawn every 1.5 s");
            Assert.That(run.Deaths, Is.GreaterThan(0), "predators did eat something");
            Assert.That(run.Stats.Total, Is.EqualTo(run.Deaths), "the counter matches the actual deaths");
            Assert.That(run.Spawned - run.Population.Animals.Count, Is.EqualTo(run.Deaths), "every missing animal is a counted death");
            Assert.That(run.MutualMeals, Is.Zero, "two predators never both die in one collision");
            Assert.That(run.MealsByTheDead, Is.Zero, "a dead animal never eats");
        }

        [UnityTest]
        public IEnumerator SameSeed_ReplaysTheSameRun()
        {
            var first = new Run(Build(seed: 7));
            yield return first.Simulate(seconds: 40f);
            first.Dispose();
            yield return null; // Destroy is deferred to the end of the frame: clear the physics scene.

            var second = new Run(Build(seed: 7));
            yield return second.Simulate(seconds: 40f);

            Assert.That(first.MealLog, Is.Not.Empty);
            Assert.That(second.MealLog, Is.EqualTo(first.MealLog));
        }

        private IObjectResolver Build(int seed)
        {
            var settings = LoadDefaultWorld();
            var installer = new ZooInstaller(settings, new FixedPlayArea(Visible, settings.ScreenMargin, settings.SpawnMargin))
            {
                Seed = seed,
                RunInPlayerLoop = false,
                WithVisuals = false
            };

            var builder = new ContainerBuilder();
            installer.Install(builder);
            var container = builder.Build();
            _containers.Add(container);
            return container;
        }

        private static WorldSettings LoadDefaultWorld()
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<WorldSettings>("Assets/ZooWorld/Content/Settings/DefaultWorld.asset");
#else
            Assert.Ignore("Content is loaded through the AssetDatabase; run in the Editor.");
            return null;
#endif
        }

        /// <summary>One simulated zoo plus the bookkeeping the invariants need.</summary>
        private sealed class Run
        {
            private readonly IObjectResolver _container;
            private readonly ZooSimulation _simulation;
            private readonly AnimalRegistry _registry;
            private readonly HashSet<(AnimalId, AnimalId)> _mealsThisStep = new HashSet<(AnimalId, AnimalId)>();

            public Run(IObjectResolver container)
            {
                _container = container;
                _simulation = container.Resolve<ZooSimulation>();
                _registry = container.Resolve<AnimalRegistry>();
                Population = container.Resolve<AnimalPopulation>();
                Stats = container.Resolve<DeathStats>();

                Population.Spawned += _ => Spawned++;
                _registry.Died += _ => Deaths++;
                container.Resolve<CollisionService>().Eaten += OnEaten;
            }

            public AnimalPopulation Population { get; }
            public DeathStats Stats { get; }
            public int Spawned { get; private set; }
            public int Deaths { get; private set; }
            public int MutualMeals { get; private set; }
            public int MealsByTheDead { get; private set; }
            public List<string> MealLog { get; } = new List<string>();

            public IEnumerator Simulate(float seconds, System.Action<float> afterStep = null)
            {
                var steps = Mathf.RoundToInt(seconds / Step);
                for (var i = 1; i <= steps; i++)
                {
                    _mealsThisStep.Clear();
                    _simulation.Step(Step);
                    Physics.Simulate(Step);
                    afterStep?.Invoke(i * Step);

                    if (i % StepsPerFrame == 0) yield return null;
                }
            }

            public void Dispose() => _container.Dispose();

            private void OnEaten(Meal meal)
            {
                if (_mealsThisStep.Contains((meal.Victim, meal.Eater))) MutualMeals++;
                if (!_registry.IsAlive(meal.Eater)) MealsByTheDead++;

                _mealsThisStep.Add((meal.Eater, meal.Victim));
                MealLog.Add($"{meal.Eater.Value}>{meal.Victim.Value}");
            }
        }
    }
}
