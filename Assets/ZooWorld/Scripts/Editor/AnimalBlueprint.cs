using System.Collections.Generic;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Configs;
using ZooWorld.Game.Movement;

namespace ZooWorld.Editor
{
    /// <summary>Everything needed to create a new animal from existing parts.</summary>
    public sealed class AnimalBlueprint
    {
        public string Name;
        public string Folder = AnimalAssetCreator.DefaultFolder;
        public Animal BasePrefab;
        public IReadOnlyList<AnimalTag> Tags = new AnimalTag[0];
        public IReadOnlyList<AnimalTag> Diet = new AnimalTag[0];
        public MovementSettings Movement;
    }
}
