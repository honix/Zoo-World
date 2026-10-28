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

        /// <summary>
        /// Resolves a contact. The outcome doesn't depend on which collider reported it:
        /// the pair is always resolved in id order, so a replay with the same seed
        /// gets the same winners even if the engine reports contacts in another order.
        /// </summary>
        public void Report(AnimalId first, AnimalId second)
        {
            if (first == second) return;
            if (second.Value < first.Value) (first, second) = (second, first);

            if (!_registry.TryGetProfile(first, out var firstProfile)) return;
            if (!_registry.TryGetProfile(second, out var secondProfile)) return;

            switch (_foodChain.Resolve(firstProfile, secondProfile))
            {
                case CollisionOutcome.FirstEatsSecond:
                    Feed(first, second);
                    break;
                case CollisionOutcome.SecondEatsFirst:
                    Feed(second, first);
                    break;
            }
        }

        private void Feed(AnimalId eater, AnimalId victim)
        {
            _registry.Kill(victim);
            Eaten?.Invoke(new Meal(eater, victim));
        }
    }
}
