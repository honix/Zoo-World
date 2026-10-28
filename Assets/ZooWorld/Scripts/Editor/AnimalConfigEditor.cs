using System.Linq;
using UnityEditor;
using ZooWorld.Game.Configs;

namespace ZooWorld.Editor
{
    /// <summary>Shows a designer what's wrong with a config, or a one-line summary when it's fine.</summary>
    [CustomEditor(typeof(AnimalConfig))]
    public sealed class AnimalConfigEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            var config = (AnimalConfig)target;
            var problems = config.Validate().ToArray();
            if (problems.Length > 0)
            {
                foreach (var problem in problems) EditorGUILayout.HelpBox(problem, MessageType.Warning);
                return;
            }

            var diet = config.Diet.Where(tag => tag != null).Select(tag => tag.name).ToArray();
            var summary = diet.Length == 0
                ? "Prey: eats nothing."
                : $"Predator: eats anything tagged {string.Join(", ", diet)}.";
            EditorGUILayout.HelpBox($"{summary}\nMoves with {config.Movement.name}.", MessageType.Info);
        }
    }
}
