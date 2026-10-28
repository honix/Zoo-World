using System;
using ZooWorld.Core.Animals;

namespace ZooWorld.Core.FoodChain
{
    /// <summary>
    /// Default rules: whoever can eat the other does.
    /// If both can, the <see cref="ITieBreaker"/> picks the survivor.
    /// </summary>
    public sealed class DietFoodChain : IFoodChain
    {
        private readonly ITieBreaker _tieBreaker;

        public DietFoodChain(ITieBreaker tieBreaker)
        {
            _tieBreaker = tieBreaker ?? throw new ArgumentNullException(nameof(tieBreaker));
        }

        public CollisionOutcome Resolve(AnimalProfile first, AnimalProfile second)
        {
            var firstEats = first.CanEat(second);
            var secondEats = second.CanEat(first);

            if (firstEats && secondEats)
            {
                return _tieBreaker.FirstWins(first, second)
                    ? CollisionOutcome.FirstEatsSecond
                    : CollisionOutcome.SecondEatsFirst;
            }

            if (firstEats) return CollisionOutcome.FirstEatsSecond;
            if (secondEats) return CollisionOutcome.SecondEatsFirst;
            return CollisionOutcome.Bounce;
        }
    }
}
