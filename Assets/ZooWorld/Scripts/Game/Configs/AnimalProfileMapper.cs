using System.Collections.Generic;
using System.Linq;
using ZooWorld.Core.Animals;

namespace ZooWorld.Game.Configs
{
    /// <summary>
    /// Adapter from authoring data to Core: Core can't see ScriptableObjects,
    /// so each <see cref="AnimalTag"/> asset gets a plain <see cref="TagId"/>.
    /// </summary>
    public sealed class AnimalProfileMapper
    {
        private readonly Dictionary<AnimalTag, TagId> _tagIds = new Dictionary<AnimalTag, TagId>();
        private readonly Dictionary<AnimalConfig, AnimalProfile> _profiles = new Dictionary<AnimalConfig, AnimalProfile>();

        public AnimalProfile GetProfile(AnimalConfig config)
        {
            if (!_profiles.TryGetValue(config, out var profile))
            {
                profile = new AnimalProfile(config.name, ToIds(config.Tags), ToIds(config.Diet));
                _profiles.Add(config, profile);
            }

            return profile;
        }

        public TagId GetTagId(AnimalTag tag)
        {
            if (!_tagIds.TryGetValue(tag, out var id))
            {
                id = new TagId(_tagIds.Count + 1);
                _tagIds.Add(tag, id);
            }

            return id;
        }

        private IEnumerable<TagId> ToIds(IEnumerable<AnimalTag> tags) =>
            tags.Where(tag => tag != null).Select(GetTagId).ToArray();
    }
}
