using ZooWorld.Core.Animals;

namespace ZooWorld.Core.FoodChain
{
    /// <summary>One animal has just eaten another.</summary>
    public readonly struct Meal
    {
        public Meal(AnimalId eater, AnimalId victim)
        {
            Eater = eater;
            Victim = victim;
        }

        public AnimalId Eater { get; }

        public AnimalId Victim { get; }
    }
}
