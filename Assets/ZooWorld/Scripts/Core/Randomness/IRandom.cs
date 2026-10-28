namespace ZooWorld.Core.Randomness
{
    /// <summary>
    /// Source of randomness. Injected everywhere instead of a global random,
    /// so a run can be replayed from its seed and tests stay deterministic.
    /// </summary>
    public interface IRandom
    {
        /// <summary>Returns a value in [0, 1).</summary>
        double NextDouble();

        /// <summary>Returns an integer in [minInclusive, maxExclusive).</summary>
        int Next(int minInclusive, int maxExclusive);
    }
}
