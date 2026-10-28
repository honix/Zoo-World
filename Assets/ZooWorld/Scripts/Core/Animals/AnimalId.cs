using System;

namespace ZooWorld.Core.Animals
{
    /// <summary>Runtime identity of one living animal, issued by <see cref="AnimalRegistry"/>.</summary>
    public readonly struct AnimalId : IEquatable<AnimalId>
    {
        public AnimalId(int value) => Value = value;

        public int Value { get; }

        public bool Equals(AnimalId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is AnimalId other && Equals(other);
        public override int GetHashCode() => Value;
        public override string ToString() => $"Animal#{Value}";

        public static bool operator ==(AnimalId left, AnimalId right) => left.Equals(right);
        public static bool operator !=(AnimalId left, AnimalId right) => !left.Equals(right);
    }
}
