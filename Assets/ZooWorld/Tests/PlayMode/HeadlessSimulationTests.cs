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
using ZooWorld.Game.Movement;
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
        private readonly List<Object> _created = new List<Object>();
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
            foreach (var asset in _created) Object.DestroyImmediate(asset);
            _created.Clear();
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
            var settings = BuildTestWorld();
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

        /// <summary>
        /// The spec's zoo (frogs and snakes, one spawn every 1-2 s) built in code with default tuning,
        /// so editing the shipped content can't change what this test checks.
        /// Only the base prefab is shared: it is the physics every animal really has.
        /// </summary>
        private WorldSettings BuildTestWorld()
        {
#if UNITY_EDITOR
            var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<Animal>("Assets/ZooWorld/Content/Prefabs/AnimalBase.prefab");
            var animal = Create<AnimalTag>("Animal");
            var frog = Create<AnimalConfig>("Frog");
            var snake = Create<AnimalConfig>("Snake");
            Configure(frog, prefab, new[] { animal }, new AnimalTag[0], Create<JumpMovementSettings>("FrogJump"));
            Configure(snake, prefab, new[] { animal }, new[] { animal }, Create<LinearMovementSettings>("SnakeCrawl"));

            var world = Create<WorldSettings>("TestWorld");
            var table = new UnityEditor.SerializedObject(world).FindProperty("_spawnTable");
            table.arraySize = 2;
            SetEntry(table.GetArrayElementAtIndex(0), frog);
            SetEntry(table.GetArrayElementAtIndex(1), snake);
            table.serializedObject.ApplyModifiedPropertiesWithoutUndo();
            return world;
#else
            Assert.Ignore("The test world is built with the Editor's SerializedObject; run in the Editor.");
            return null;
#endif
        }

        private T Create<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            asset.name = name;
            _created.Add(asset);
            return asset;
        }

#if UNITY_EDITOR
        private static void Configure(AnimalConfig config, Animal prefab, AnimalTag[] tags, AnimalTag[] diet, MovementSettings movement)
        {
            var serialized = new UnityEditor.SerializedObject(config);
            serialized.FindProperty("_prefab").objectReferenceValue = prefab;
            serialized.FindProperty("_movement").objectReferenceValue = movement;
            Fill(serialized.FindProperty("_tags"), tags);
            Fill(serialized.FindProperty("_diet"), diet);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Fill(UnityEditor.SerializedProperty array, Object[] items)
        {
            array.arraySize = items.Length;
            for (var i = 0; i < items.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        private static void SetEntry(UnityEditor.SerializedProperty entry, AnimalConfig animal)
        {
            entry.FindPropertyRelative(nameof(WorldSettings.SpawnTableEntry.Animal)).objectReferenceValue = animal;
            entry.FindPropertyRelative(nameof(WorldSettings.SpawnTableEntry.Weight)).floatValue = 1f;
        }
#endif

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
