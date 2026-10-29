using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ZooWorld.Core.Animals;
using ZooWorld.Core.Randomness;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Configs;
using Object = UnityEngine.Object;

namespace ZooWorld.Game.Tests
{
    /// <summary>
    /// Contracts every content asset in the project must keep. Each test scans the project
    /// when it runs, so new animals and world presets are checked at once, without a script
    /// reload or a new test. A failure lists every broken asset as "Name: problem".
    /// </summary>
    public class ContentContractTests
    {
        [Test]
        public void Project_HasContent()
        {
            Assert.That(FindAll<AnimalConfig>(), Is.Not.Empty);
            Assert.That(FindAll<WorldSettings>(), Is.Not.Empty);
        }

        [Test]
        public void Animals_ConfigsAreValid()
        {
            Assert.That(ProblemsIn(FindAll<AnimalConfig>(), config => config.Validate()), Is.Empty);
        }

        [Test]
        public void Animals_PrefabsFollowTheBaseLayout()
        {
            Assert.That(ProblemsIn(FindAll<AnimalConfig>(), LayoutProblems), Is.Empty);
        }

        [Test]
        public void Animals_CanBeSpawned()
        {
            Assert.That(ProblemsIn(FindAll<AnimalConfig>(), SpawnProblems), Is.Empty);
        }

        [Test]
        public void Worlds_SettingsAreValid()
        {
            Assert.That(ProblemsIn(FindAll<WorldSettings>(), WorldProblems), Is.Empty);
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

        private static IEnumerable<string> LayoutProblems(AnimalConfig config)
        {
            var prefab = config.Prefab;
            if (prefab == null) yield break; // Reported by Animals_ConfigsAreValid.

            var body = prefab.GetComponent<Rigidbody>();
            if (body == null)
            {
                yield return "needs a Rigidbody on the root";
                yield break;
            }

            if (body.useGravity) yield return "the Rigidbody uses gravity, but animals live on the ground plane";
            if (body.isKinematic) yield return "the Rigidbody is kinematic, but animals collide by physics";
            if ((body.constraints & RigidbodyConstraints.FreezePositionY) == 0) yield return "the Rigidbody doesn't freeze Y position";
            if ((body.constraints & RigidbodyConstraints.FreezeRotation) != RigidbodyConstraints.FreezeRotation) yield return "the Rigidbody doesn't freeze rotation";

            if (!prefab.GetComponents<Collider>().Any(collider => collider.enabled && !collider.isTrigger))
                yield return "needs a solid collider on the root";

            var visual = prefab.transform.Find(Animal.VisualName);
            if (visual == null)
            {
                yield return $"the look belongs on a child named {Animal.VisualName}";
                yield break;
            }

            var visualColliders = visual.GetComponentsInChildren<Collider>(true);
            if (visualColliders.Length > 0)
                yield return $"the Visual must not change physics, but has colliders on {Names(visualColliders)}";

            // Views animate from the scale they find at start; a collapsed one stays invisible.
            var collapsed = visual.GetComponentsInChildren<Transform>(true).Where(part => part.localScale.sqrMagnitude < 1e-6f).ToArray();
            if (collapsed.Length > 0) yield return $"parts of the Visual have zero scale: {Names(collapsed)}";
        }

        private static IEnumerable<string> SpawnProblems(AnimalConfig config)
        {
            if (config.Prefab == null || config.Movement == null) return Enumerable.Empty<string>(); // Reported by Animals_ConfigsAreValid.

            var problems = new List<string>();
            var animal = Object.Instantiate(config.Prefab);
            try
            {
                var position = new Vector3(1f, 0f, 2f);
                animal.Activate(new AnimalId(1), config, config.Movement.CreateMovement(new SeededRandom(0)), null, position);

                if (!animal.IsAlive) problems.Add("is not alive after spawning");
                if (Vector3.Distance(animal.Position, position) > 1e-4f) problems.Add($"spawned at {animal.Position} instead of {position}");
                if (Mathf.Abs(animal.Movement.Heading.magnitude - 1f) > 1e-4f) problems.Add("its movement has no valid heading");
            }
            catch (Exception exception)
            {
                problems.Add($"throws on spawn: {exception.Message}");
            }
            finally
            {
                Object.DestroyImmediate(animal.gameObject);
            }

            return problems;
        }

        private static IEnumerable<string> WorldProblems(WorldSettings world)
        {
            foreach (var problem in world.Validate()) yield return problem;

            foreach (var entry in world.SpawnTable.Where(entry => entry.Animal != null))
            {
                foreach (var problem in entry.Animal.Validate()) yield return $"{entry.Animal.name} in the spawn table: {problem}";
            }
        }

        private static List<string> ProblemsIn<T>(IEnumerable<T> assets, Func<T, IEnumerable<string>> check) where T : Object =>
            assets.SelectMany(asset => check(asset).Select(problem => $"{asset.name}: {problem}")).ToList();

        private static string Names(IEnumerable<Component> parts) => string.Join(", ", parts.Select(part => part.name));

        private static IEnumerable<T> FindAll<T>() where T : Object =>
            AssetDatabase.FindAssets($"t:{typeof(T).Name}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null);
    }
}
