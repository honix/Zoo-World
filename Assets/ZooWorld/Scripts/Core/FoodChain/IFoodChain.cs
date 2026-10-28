using ZooWorld.Core.Animals;

namespace ZooWorld.Core.FoodChain
{
    /// <summary>
    /// The single place that decides what happens when two animals meet.
    /// New kinds of reactions (e.g. poisonous prey) are added by implementing
    /// or decorating this interface, never inside the animals.
    /// </summary>
    public interface IFoodChain
    {
        CollisionOutcome Resolve(AnimalProfile first, AnimalProfile second);
    }
}
