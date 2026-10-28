using System.Collections;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;
using ZooWorld.Core.Animals;
using ZooWorld.Core.FoodChain;
using ZooWorld.Core.Stats;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Configs;
using ZooWorld.Game.Infrastructure;
using ZooWorld.Game.UI;

namespace ZooWorld.Game.PlayModeTests
{
    /// <summary>End to end in the real Zoo scene: spawn, collision, death, counter, label.</summary>
    public class ZooSceneSmokeTests
    {
        private const float Timeout = 10f;

        [UnityTest]
        public IEnumerator PredatorEatsPrey_CounterAndLabelUpdate()
        {
            yield return LoadZooScene();
            var container = Object.FindFirstObjectByType<ZooLifetimeScope>().Container;
            var population = container.Resolve<AnimalPopulation>();
            var stats = container.Resolve<DeathStats>();
            var (preyConfig, predatorConfig) = PickPreyAndPredator(container);

            var predator = population.Spawn(predatorConfig, new Vector3(-1.5f, 0f, 0f));
            var prey = population.Spawn(preyConfig, new Vector3(1.5f, 0f, 0f));
            var preyId = prey.Id;
            Meal? meal = null;
            container.Resolve<CollisionService>().Eaten += m =>
            {
                if (m.Victim == preyId) meal = m;
            };

            for (var elapsed = 0f; meal == null && elapsed < Timeout; elapsed += Time.fixedDeltaTime)
            {
                if (predator.IsAlive && prey.IsAlive) predator.Movement.Steer(prey.Position - predator.Position);
                yield return new WaitForFixedUpdate();
            }
            yield return null;

            Assert.That(meal, Is.Not.Null, "the prey was never eaten");
            Assert.That(prey.IsAlive, Is.False);
            Assert.That(stats.DeadPrey, Is.GreaterThanOrEqualTo(1));
            Assert.That(CounterText("Prey Count"), Is.EqualTo(stats.DeadPrey.ToString()));
            Assert.That(CounterText("Predators Count"), Is.EqualTo(stats.DeadPredators.ToString()));
            Assert.That(Object.FindObjectsByType<TastyLabel>(FindObjectsSortMode.None), Is.Not.Empty, "a Tasty! label is shown");
        }

        [UnityTearDown]
        public IEnumerator UnloadZooScene()
        {
            var zoo = SceneManager.GetSceneByName("Zoo");
            if (!zoo.isLoaded) yield break;

            SceneManager.SetActiveScene(SceneManager.CreateScene("Empty"));
            yield return SceneManager.UnloadSceneAsync(zoo);
        }

        private static IEnumerator LoadZooScene()
        {
            yield return SceneManager.LoadSceneAsync("Zoo", LoadSceneMode.Single);
            yield return null;
        }

        private static (AnimalConfig prey, AnimalConfig predator) PickPreyAndPredator(IObjectResolver container)
        {
            var mapper = container.Resolve<AnimalProfileMapper>();
            var animals = container.Resolve<WorldSettings>().SpawnTable.Select(entry => entry.Animal).ToArray();
            return (animals.First(a => mapper.GetProfile(a).Role == AnimalRole.Prey),
                animals.First(a => mapper.GetProfile(a).Role == AnimalRole.Predator));
        }

        private static string CounterText(string name) =>
            Object.FindFirstObjectByType<DeathCounterView>()
                .GetComponentsInChildren<TMP_Text>()
                .Single(text => text.name == name).text;
    }
}
