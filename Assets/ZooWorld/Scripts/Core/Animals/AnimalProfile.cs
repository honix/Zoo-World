using System;
using System.Collections.Generic;
using System.Linq;

namespace ZooWorld.Core.Animals
{
    /// <summary>
    /// What an animal is (its tags) and what it eats (its diet).
    /// "A snake eats everything, a frog eats nothing" and "a spider eats frogs
    /// but not snakes" are both just data here.
    /// </summary>
    public sealed class AnimalProfile
    {
        private readonly TagId[] _tags;
        private readonly TagId[] _diet;

        public AnimalProfile(string name, IEnumerable<TagId> tags, IEnumerable<TagId> diet)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            _tags = tags?.Distinct().ToArray() ?? throw new ArgumentNullException(nameof(tags));
            _diet = diet?.Distinct().ToArray() ?? throw new ArgumentNullException(nameof(diet));
        }

        public string Name { get; }

        public IReadOnlyList<TagId> Tags => _tags;

        public IReadOnlyList<TagId> Diet => _diet;

        /// <summary>An animal that can eat anything at all counts as a predator.</summary>
        public AnimalRole Role => _diet.Length > 0 ? AnimalRole.Predator : AnimalRole.Prey;

        public bool HasTag(TagId tag) => Array.IndexOf(_tags, tag) >= 0;

        public bool CanEat(AnimalProfile other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            foreach (var food in _diet)
            {
                if (other.HasTag(food)) return true;
            }

            return false;
        }

        public override string ToString() => Name;
    }
}
