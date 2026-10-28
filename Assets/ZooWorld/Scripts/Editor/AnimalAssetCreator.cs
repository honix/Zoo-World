using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Configs;

namespace ZooWorld.Editor
{
    /// <summary>
    /// Creates a new animal as content: a prefab variant of the base animal and an
    /// <see cref="AnimalConfig"/> pointing at it, side by side in their own folder.
    /// </summary>
    public static class AnimalAssetCreator
    {
        public const string DefaultFolder = "Assets/ZooWorld/Content/Animals";

        public static IEnumerable<string> Validate(AnimalBlueprint blueprint)
        {
            if (string.IsNullOrWhiteSpace(blueprint.Name)) yield return "Give the animal a name.";
            else if (blueprint.Name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) yield return "The name can't be used as a file name.";
            else if (AssetDatabase.IsValidFolder(AnimalFolder(blueprint))) yield return $"{AnimalFolder(blueprint)} already exists.";

            if (blueprint.BasePrefab == null) yield return "Pick a base prefab.";
            if (blueprint.Movement == null) yield return "Pick a movement.";
            if (!blueprint.Tags.Any(tag => tag != null)) yield return "Add at least one tag.";
        }

        public static AnimalConfig Create(AnimalBlueprint blueprint)
        {
            var problems = Validate(blueprint).ToArray();
            if (problems.Length > 0) throw new System.ArgumentException(string.Join(" ", problems), nameof(blueprint));

            var folder = AnimalFolder(blueprint);
            EnsureFolder(folder);

            var prefab = CreatePrefabVariant(blueprint.BasePrefab, $"{folder}/{blueprint.Name}.prefab");
            var config = ScriptableObject.CreateInstance<AnimalConfig>();
            AssetDatabase.CreateAsset(config, $"{folder}/{blueprint.Name}.asset");

            var serialized = new SerializedObject(config);
            serialized.FindProperty("_prefab").objectReferenceValue = prefab;
            serialized.FindProperty("_movement").objectReferenceValue = blueprint.Movement;
            Fill(serialized.FindProperty("_tags"), blueprint.Tags);
            Fill(serialized.FindProperty("_diet"), blueprint.Diet);
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.SaveAssets();
            return config;
        }

        /// <summary>Adds the animal to a world's spawn table, or updates its weight if it's already there.</summary>
        public static void AddToSpawnTable(WorldSettings world, AnimalConfig animal, float weight)
        {
            var serialized = new SerializedObject(world);
            var table = serialized.FindProperty("_spawnTable");

            var index = Enumerable.Range(0, table.arraySize)
                .Where(i => table.GetArrayElementAtIndex(i).FindPropertyRelative("Animal").objectReferenceValue == animal)
                .DefaultIfEmpty(-1)
                .First();
            if (index < 0)
            {
                index = table.arraySize;
                table.arraySize++;
            }

            var entry = table.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("Animal").objectReferenceValue = animal;
            entry.FindPropertyRelative("Weight").floatValue = weight;
            serialized.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        private static string AnimalFolder(AnimalBlueprint blueprint) => $"{blueprint.Folder.TrimEnd('/')}/{blueprint.Name}";

        private static Animal CreatePrefabVariant(Animal basePrefab, string path)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab.gameObject);
            try
            {
                instance.name = Path.GetFileNameWithoutExtension(path);
                return PrefabUtility.SaveAsPrefabAsset(instance, path).GetComponent<Animal>();
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        private static void Fill(SerializedProperty array, IReadOnlyList<AnimalTag> tags)
        {
            var items = tags.Where(tag => tag != null).Distinct().ToArray();
            array.arraySize = items.Length;
            for (var i = 0; i < items.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;

            var parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }
    }
}
