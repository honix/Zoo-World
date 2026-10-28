using System;
using ZooWorld.Core.Animals;

namespace ZooWorld.Core.FoodChain
{
    /// <summary>
    /// Turns physical contacts into food chain outcomes.
    /// Unity reports every contact twice (once per collider). The loser is killed
    /// on the first report, so the mirrored second report finds a dead animal and
    /// is ignored: two predators can never eat each other.
    /// </summary>
    public sealed class CollisionService
    {
        private readonly AnimalRegistry _registry;
        private readonly IFoodChain _foodChain;

        public CollisionService(AnimalRegistry registry, IFoodChain foodChain)
        {
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _foodChain = foodChain ?? throw new ArgumentNullException(nameof(foodChain));
        }

        /// <summary>Raised after the victim is already dead.</summary>
        public event Action<Meal> Eaten;

        public CollisionOutcome Report(AnimalId first, AnimalId second)
        {
            if (first == second) return CollisionOutcome.Bounce;
            if (!_registry.TryGetProfile(first, out var firstProfile)) return CollisionOutcome.Bounce;
            if (!_registry.TryGetProfile(second, out var secondProfile)) return CollisionOutcome.Bounce;

            var outcome = _foodChain.Resolve(firstProfile, secondProfile);
            switch (outcome)
            {
                case CollisionOutcome.FirstEatsSecond:
                    Feed(first, second);
                    break;
                case CollisionOutcome.SecondEatsFirst:
                    Feed(second, first);
                    break;
            }

            return outcome;
        }

        private void Feed(AnimalId eater, AnimalId victim)
        {
            _registry.Kill(victim);
            Eaten?.Invoke(new Meal(eater, victim));
        }
    }
}
