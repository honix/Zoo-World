using System.Linq;
using UnityEditor;
using ZooWorld.Game.Configs;

namespace ZooWorld.Editor
{
    /// <summary>Flags world settings problems and shows each animal's share of spawns.</summary>
    [CustomEditor(typeof(WorldSettings))]
    public sealed class WorldSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();

            var world = (WorldSettings)target;
            var problems = world.Validate().ToArray();
            foreach (var problem in problems) EditorGUILayout.HelpBox(problem, MessageType.Warning);
            if (problems.Length > 0) return;

            var entries = world.SpawnTable.Where(entry => entry.Animal != null && entry.Weight > 0f).ToArray();
            var total = entries.Sum(entry => entry.Weight);
            var shares = entries.Select(entry => $"{entry.Animal.name} {entry.Weight / total:P0}");
            EditorGUILayout.HelpBox($"Spawn shares: {string.Join(", ", shares)}", MessageType.Info);
        }
    }
}
