namespace ZooWorld.Core.FoodChain
{
    public enum CollisionOutcome
    {
        /// <summary>Neither eats the other; physics pushes them apart.</summary>
        Bounce,
        FirstEatsSecond,
        SecondEatsFirst
    }
}
