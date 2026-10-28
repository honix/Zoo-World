using System;
using ZooWorld.Core.Animals;
using ZooWorld.Core.Randomness;

namespace ZooWorld.Core.FoodChain
{
    /// <summary>A fair 50/50 coin flip, reproducible through the tie-break random stream.</summary>
    public sealed class CoinFlipTieBreaker : ITieBreaker
    {
        private readonly IRandom _random;

        public CoinFlipTieBreaker(IRandom random)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        public bool FirstWins(AnimalProfile first, AnimalProfile second) => _random.Chance(0.5);
    }
}
