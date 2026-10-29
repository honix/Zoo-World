using System.Linq;
using UnityEditor;
using UnityEngine;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Configs;
using ZooWorld.Game.Movement;

namespace ZooWorld.Editor
{
    /// <summary>
    /// Zoo World > Create Animal: a new animal from existing parts in one click,
    /// a config plus a prefab variant of the base animal, ready for an artist's Visual.
    /// </summary>
    public sealed class CreateAnimalWizard : ScriptableWizard
    {
        public const string BasePrefabPath = "Assets/ZooWorld/Content/Prefabs/AnimalBase.prefab";

        [Tooltip("Used for the folder, the prefab and the config.")]
        [SerializeField] private string _name = "NewAnimal";

        [SerializeField] private Animal _basePrefab;

        [Tooltip("What this animal is.")]
        [SerializeField] private AnimalTag[] _tags = new AnimalTag[0];

        [Tooltip("Tags it eats. Leave empty for prey.")]
        [SerializeField] private AnimalTag[] _diet = new AnimalTag[0];

        [SerializeField] private MovementSettings _movement;

        [Header("Optional")]
        [Tooltip("Also add the new animal to this world's spawn table.")]
        [SerializeField] private WorldSettings _addToWorld;

        [SerializeField, Min(0f)] private float _spawnWeight = 1f;

        [MenuItem("Zoo World/Create Animal...", priority = 0)]
        private static void Open()
        {
            var wizard = DisplayWizard<CreateAnimalWizard>("Create Animal", "Create");
            wizard._basePrefab = AssetDatabase.LoadAssetAtPath<Animal>(BasePrefabPath);
            wizard.OnWizardUpdate();
        }

        private AnimalBlueprint Blueprint => new AnimalBlueprint
        {
            Name = _name?.Trim(),
            BasePrefab = _basePrefab,
            Tags = _tags,
            Diet = _diet,
            Movement = _movement
        };

        private void OnWizardUpdate()
        {
            helpString = "Creates a prefab variant and an AnimalConfig in their own folder. " +
                         "Replace the variant's Visual child with the new look; logic and physics stay on the root.";
            var problems = AnimalAssetCreator.Validate(Blueprint).ToArray();
            errorString = string.Join("\n", problems);
            isValid = problems.Length == 0;
        }

        private void OnWizardCreate()
        {
            var config = AnimalAssetCreator.Create(Blueprint);
            if (_addToWorld != null) AnimalAssetCreator.AddToSpawnTable(_addToWorld, config, _spawnWeight);

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }
    }
}
