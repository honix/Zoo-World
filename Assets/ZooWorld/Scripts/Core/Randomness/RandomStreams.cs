namespace ZooWorld.Core.Randomness
{
    /// <summary>
    /// Independent random streams derived from one master seed.
    /// Each system draws from its own stream, so extra draws in one system
    /// (e.g. movement) never shift the sequence of another (e.g. spawning).
    /// </summary>
    public sealed class RandomStreams
    {
        public RandomStreams(int masterSeed)
        {
            MasterSeed = masterSeed;
            Spawning = new SeededRandom(DeriveSeed(masterSeed, 1));
            Movement = new SeededRandom(DeriveSeed(masterSeed, 2));
            TieBreak = new SeededRandom(DeriveSeed(masterSeed, 3));
        }

        public int MasterSeed { get; }

        /// <summary>What spawns, when and where.</summary>
        public IRandom Spawning { get; }

        /// <summary>Headings, jump directions, steering targets.</summary>
        public IRandom Movement { get; }

        /// <summary>Who survives when two animals can eat each other.</summary>
        public IRandom TieBreak { get; }

        /// <summary>Mixes the master seed with a stream index (a MurmurHash3-style finalizer).</summary>
        public static int DeriveSeed(int masterSeed, int streamIndex)
        {
            unchecked
            {
                var x = (uint)masterSeed + 0x9E3779B9u * (uint)streamIndex;
                x ^= x >> 16;
                x *= 0x85EBCA6Bu;
                x ^= x >> 13;
                x *= 0xC2B2AE35u;
                x ^= x >> 16;
                return (int)x;
            }
        }
    }
}
