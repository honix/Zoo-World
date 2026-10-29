using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using ZooWorld.Editor;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Configs;
using ZooWorld.Game.Movement;
using ZooWorld.Game.Tests.Fakes;

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
            // The tag and movement are the test's own, so renaming or retuning content can't break it.
            // The base prefab is the one the wizard really uses.
            AssetDatabase.CreateFolder(Path.GetDirectoryName(Folder)?.Replace('\\', '/'), Path.GetFileName(Folder));
            var blueprint = new AnimalBlueprint
            {
                Name = "Turtle",
                Folder = Folder,
                BasePrefab = AssetDatabase.LoadAssetAtPath<Animal>(CreateAnimalWizard.BasePrefabPath),
                Tags = new[] { Save(TestAssets.Tag("Animal")) },
                Movement = Save(ScriptableObject.CreateInstance<LinearMovementSettings>())
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

        private static T Save<T>(T asset) where T : ScriptableObject
        {
            AssetDatabase.CreateAsset(asset, $"{Folder}/{typeof(T).Name}.asset");
            return asset;
        }
    }
}
