using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Movement;

namespace ZooWorld.Game.Configs
{
    /// <summary>
    /// Everything that makes an animal: its prefab, what it is, what it eats and how it moves.
    /// A new animal built from existing parts is a new config and a prefab variant, no code.
    /// </summary>
    [CreateAssetMenu(menuName = "Zoo World/Animal", fileName = "Animal")]
    public sealed class AnimalConfig : ScriptableObject
    {
        [SerializeField] private Animal _prefab;

        [Tooltip("What this animal is. Other animals' diets refer to these tags.")]
        [SerializeField] private AnimalTag[] _tags = new AnimalTag[0];

        [Tooltip("Tags this animal eats. Empty means it's prey and eats nothing.")]
        [SerializeField] private AnimalTag[] _diet = new AnimalTag[0];

        [SerializeField] private MovementSettings _movement;

        public Animal Prefab => _prefab;
        public IReadOnlyList<AnimalTag> Tags => _tags;
        public IReadOnlyList<AnimalTag> Diet => _diet;
        public MovementSettings Movement => _movement;

        /// <summary>Human-readable problems with this config; empty when it can be spawned.</summary>
        public IEnumerable<string> Validate()
        {
            if (_prefab == null) yield return "Prefab is missing.";
            if (_movement == null) yield return "Movement settings are missing.";
            if (_tags == null || _tags.Length == 0) yield return "An animal needs at least one tag.";
            if (_tags != null && _tags.Any(tag => tag == null)) yield return "Tags contain an empty slot.";
            if (_diet != null && _diet.Any(tag => tag == null)) yield return "Diet contains an empty slot.";

            if (_movement == null) yield break;
            foreach (var problem in _movement.Validate())
            {
                yield return $"Movement '{_movement.name}': {problem}";
            }
        }
    }
}
