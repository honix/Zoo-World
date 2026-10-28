namespace ZooWorld.Core.Spawning
{
    public readonly struct SpawnEntry<T>
    {
        public SpawnEntry(T item, double weight)
        {
            Item = item;
            Weight = weight;
        }

        public T Item { get; }

        /// <summary>Relative chance; zero disables the entry.</summary>
        public double Weight { get; }
    }
}
