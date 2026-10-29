using System;

namespace ZooWorld.Core.Animals
{
    /// <summary>
    /// Engine-free identity of an animal tag ("Frog", "Reptile", "Insect"...).
    /// The game maps its tag assets onto these ids.
    /// </summary>
    public readonly struct TagId : IEquatable<TagId>
    {
        /// <param name="name">For logs and the debugger only: equality ignores it.</param>
        public TagId(int value, string name = null)
        {
            Value = value;
            Name = name;
        }

        public int Value { get; }

        public string Name { get; }

        public bool Equals(TagId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is TagId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => Name ?? $"Tag#{Value}";

        public static bool operator ==(TagId left, TagId right) => left.Equals(right);
        public static bool operator !=(TagId left, TagId right) => !left.Equals(right);
    }
}
