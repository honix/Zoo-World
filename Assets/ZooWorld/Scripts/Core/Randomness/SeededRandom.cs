using System;

namespace ZooWorld.Core.Randomness
{
    /// <summary><see cref="IRandom"/> backed by <see cref="System.Random"/> with a fixed seed.</summary>
    public sealed class SeededRandom : IRandom
    {
        private readonly Random _random;

        public SeededRandom(int seed)
        {
            Seed = seed;
            _random = new Random(seed);
        }

        public int Seed { get; }

        public double NextDouble() => _random.NextDouble();

        public int Next(int minInclusive, int maxExclusive) => _random.Next(minInclusive, maxExclusive);
    }
}
