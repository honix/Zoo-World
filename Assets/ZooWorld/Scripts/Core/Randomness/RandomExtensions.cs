namespace ZooWorld.Core.Randomness
{
    public static class RandomExtensions
    {
        /// <summary>Returns a value in [min, max).</summary>
        public static double Range(this IRandom random, double min, double max) =>
            min + (max - min) * random.NextDouble();

        /// <summary>Returns true with the given probability (0..1).</summary>
        public static bool Chance(this IRandom random, double probability) =>
            random.NextDouble() < probability;
    }
}
