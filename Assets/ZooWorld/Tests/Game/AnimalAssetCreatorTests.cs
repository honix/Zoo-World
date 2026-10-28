using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ZooWorld.Editor;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Configs;
using ZooWorld.Game.Movement;

namespace ZooWorld.Game.Tests
{
    /// <summary>What the Create Animal wizard produces must pass the same contracts as hand-made content.</summary>
    public class AnimalAssetCreatorTests
    {
        private const string Folder = "Assets/ZooWorld/Tests/Game/Generated";

        [TearDown]
        public void TearDown() => AssetDatabase.DeleteAsset(Folder);

        [Test]
        public void CreatesAValidAnimalFromExistingParts()
        {
            var blueprint = new AnimalBlueprint
            {
                Name = "Turtle",
                Folder = Folder,
                BasePrefab = Load<Animal>("Assets/ZooWorld/Content/Prefabs/AnimalBase.prefab"),
                Tags = new[] { Load<AnimalTag>("Assets/ZooWorld/Content/Tags/Animal.asset") },
                Movement = Load<MovementSettings>("Assets/ZooWorld/Content/Movement/SnakeCrawl.asset")
            };

            var config = AnimalAssetCreator.Create(blueprint);

            Assert.That(config.Validate(), Is.Empty);
            Assert.That(AssetDatabase.GetAssetPath(config), Is.EqualTo($"{Folder}/Turtle/Turtle.asset"));
            Assert.That(PrefabUtility.GetPrefabAssetType(config.Prefab), Is.EqualTo(PrefabAssetType.Variant));
            Assert.That(PrefabUtility.GetCorrespondingObjectFromSource(config.Prefab.gameObject), Is.EqualTo(blueprint.BasePrefab.gameObject));
        }

        [Test]
        public void IncompleteBlueprint_IsRejected()
        {
            var blueprint = new AnimalBlueprint { Name = "", Folder = Folder };

            Assert.That(AnimalAssetCreator.Validate(blueprint).Count(), Is.EqualTo(4));
            Assert.Throws<System.ArgumentException>(() => AnimalAssetCreator.Create(blueprint));
        }

        private static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);
    }
}
