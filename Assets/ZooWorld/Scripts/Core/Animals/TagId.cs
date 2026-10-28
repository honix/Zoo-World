using System;

namespace ZooWorld.Core.Animals
{
    /// <summary>
    /// Engine-free identity of an animal tag ("Frog", "Reptile", "Insect"...).
    /// The game maps its tag assets onto these ids.
    /// </summary>
    public readonly struct TagId : IEquatable<TagId>
    {
        public TagId(int value) => Value = value;

        public int Value { get; }

        public bool Equals(TagId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is TagId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => $"Tag#{Value}";

        public static bool operator ==(TagId left, TagId right) => left.Equals(right);
        public static bool operator !=(TagId left, TagId right) => !left.Equals(right);
    }
}
