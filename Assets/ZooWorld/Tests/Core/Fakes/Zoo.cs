using ZooWorld.Core.Animals;

namespace ZooWorld.Core.Tests.Fakes
{
    /// <summary>
    /// A small cast of animals for tests, built only from tags and diets
    /// the same way the game builds them from config assets.
    /// </summary>
    public static class Zoo
    {
        public static readonly TagId AnimalTag = new TagId(1);
        public static readonly TagId FrogTag = new TagId(2);
        public static readonly TagId SnakeTag = new TagId(3);
        public static readonly TagId SpiderTag = new TagId(4);

        /// <summary>Prey: eats nothing.</summary>
        public static AnimalProfile Frog() =>
            new AnimalProfile("Frog", new[] { AnimalTag, FrogTag }, new TagId[0]);

        /// <summary>Predator: eats any animal.</summary>
        public static AnimalProfile Snake() =>
            new AnimalProfile("Snake", new[] { AnimalTag, SnakeTag }, new[] { AnimalTag });

        /// <summary>Predator with a narrow diet: eats frogs, but not snakes.</summary>
        public static AnimalProfile Spider() =>
            new AnimalProfile("Spider", new[] { AnimalTag, SpiderTag }, new[] { FrogTag });
    }
}
