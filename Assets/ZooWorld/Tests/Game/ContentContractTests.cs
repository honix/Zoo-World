using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ZooWorld.Core.Animals;
using ZooWorld.Core.Randomness;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Configs;

namespace ZooWorld.Game.Tests
{
    /// <summary>
    /// Contracts every content asset in the project must keep. New animals and
    /// world presets are picked up automatically, no test needs to be written for them.
    /// </summary>
    public class ContentContractTests
    {
        private static IEnumerable<TestCaseData> Animals() =>
            FindAll<AnimalConfig>().Select(config => new TestCaseData(config).SetName(config.name));

        private static IEnumerable<TestCaseData> Worlds() =>
            FindAll<WorldSettings>().Select(world => new TestCaseData(world).SetName(world.name));

        [Test]
        public void Project_HasContent()
        {
            Assert.That(FindAll<AnimalConfig>(), Is.Not.Empty);
            Assert.That(FindAll<WorldSettings>(), Is.Not.Empty);
        }

        [TestCaseSource(nameof(Animals))]
        public void Animal_ConfigIsValid(AnimalConfig config)
        {
            Assert.That(config.Validate(), Is.Empty);
        }

        [TestCaseSource(nameof(Animals))]
        public void Animal_PrefabFollowsTheBaseLayout(AnimalConfig config)
        {
            var prefab = config.Prefab;
            Assume.That(prefab, Is.Not.Null, "covered by Animal_ConfigIsValid");

            var body = prefab.GetComponent<Rigidbody>();
            Assert.That(body.useGravity, Is.False, "animals live on the ground plane");
            Assert.That(body.isKinematic, Is.False, "animals collide by physics");
            Assert.That(body.constraints & RigidbodyConstraints.FreezePositionY, Is.EqualTo(RigidbodyConstraints.FreezePositionY));
            Assert.That(body.constraints & RigidbodyConstraints.FreezeRotation, Is.EqualTo(RigidbodyConstraints.FreezeRotation));

            var colliders = prefab.GetComponents<Collider>();
            Assert.That(colliders.Any(collider => collider.enabled && !collider.isTrigger), Is.True, "needs a solid collider on the root");

            var visual = prefab.transform.Find(Animal.VisualName);
            Assert.That(visual, Is.Not.Null, "the look belongs on a child named Visual");
            Assert.That(visual.GetComponentsInChildren<Collider>(true), Is.Empty, "the Visual must not change physics");

            // Views animate from the scale they find at start; a collapsed one stays invisible.
            var collapsed = visual.GetComponentsInChildren<Transform>(true).Where(part => part.localScale.sqrMagnitude < 1e-6f);
            Assert.That(collapsed.Select(part => part.name), Is.Empty, "parts of the Visual have zero scale");
        }

        [TestCaseSource(nameof(Animals))]
        public void Animal_CanBeSpawned(AnimalConfig config)
        {
            Assume.That(config.Prefab, Is.Not.Null, "covered by Animal_ConfigIsValid");

            var animal = Object.Instantiate(config.Prefab);
            try
            {
                var position = new Vector3(1f, 0f, 2f);
                animal.Activate(new AnimalId(1), config, config.Movement.CreateMovement(new SeededRandom(0)), null, position);

                Assert.That(animal.IsAlive, Is.True);
                Assert.That(Vector3.Distance(animal.Position, position), Is.LessThan(1e-4f));
                Assert.That(animal.Movement.Heading.magnitude, Is.EqualTo(1f).Within(1e-4f));
            }
            finally
            {
                Object.DestroyImmediate(animal.gameObject);
            }
        }

        [TestCaseSource(nameof(Worlds))]
        public void World_SettingsAreValid(WorldSettings world)
        {
            Assert.That(world.Validate(), Is.Empty);
            foreach (var entry in world.SpawnTable.Where(entry => entry.Animal != null))
            {
                Assert.That(entry.Animal.Validate(), Is.Empty, $"{entry.Animal.name} in the spawn table");
            }
        }

        [Test]
        public void DefaultWorld_MatchesTheSpec()
        {
            var world = FindAll<WorldSettings>().Single(settings => settings.name == "DefaultWorld");
            var mapper = new AnimalProfileMapper();
            var animals = world.SpawnTable.Select(entry => entry.Animal).ToArray();

            // "Every 1-2 seconds one animal appears": frogs (prey) and snakes (predators).
            Assert.That(world.MinSpawnInterval, Is.EqualTo(1f));
            Assert.That(world.MaxSpawnInterval, Is.EqualTo(2f));
            Assert.That(animals.Select(animal => animal.name), Is.EquivalentTo(new[] { "Frog", "Snake" }));

            var frog = animals.Single(animal => animal.name == "Frog");
            var snake = animals.Single(animal => animal.name == "Snake");
            Assert.That(mapper.GetProfile(frog).Role, Is.EqualTo(AnimalRole.Prey));
            Assert.That(mapper.GetProfile(snake).Role, Is.EqualTo(AnimalRole.Predator));
            Assert.That(frog.Movement, Is.TypeOf<Movement.JumpMovementSettings>());
            Assert.That(snake.Movement, Is.TypeOf<Movement.LinearMovementSettings>());
        }

        private static IEnumerable<T> FindAll<T>() where T : Object =>
            AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null);
    }
}
