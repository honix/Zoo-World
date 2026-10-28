using UnityEditor;
using UnityEngine;
using ZooWorld.Game.Configs;

namespace ZooWorld.Game.Tests.Fakes
{
    /// <summary>In-memory config assets for tests that don't need the project's content.</summary>
    public static class TestAssets
    {
        public static AnimalTag Tag(string name)
        {
            var tag = ScriptableObject.CreateInstance<AnimalTag>();
            tag.name = name;
            return tag;
        }

        public static AnimalConfig Animal(string name, AnimalTag[] tags, AnimalTag[] diet)
        {
            var config = ScriptableObject.CreateInstance<AnimalConfig>();
            config.name = name;

            var serialized = new SerializedObject(config);
            Fill(serialized.FindProperty("_tags"), tags);
            Fill(serialized.FindProperty("_diet"), diet);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return config;
        }

        private static void Fill(SerializedProperty array, Object[] items)
        {
            array.arraySize = items.Length;
            for (var i = 0; i < items.Length; i++)
            {
                array.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
        }
    }
}
