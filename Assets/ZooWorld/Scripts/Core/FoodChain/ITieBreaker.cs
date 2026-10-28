using ZooWorld.Core.Animals;

namespace ZooWorld.Core.FoodChain
{
    /// <summary>Picks the survivor when two animals can eat each other.</summary>
    public interface ITieBreaker
    {
        bool FirstWins(AnimalProfile first, AnimalProfile second);
    }
}
