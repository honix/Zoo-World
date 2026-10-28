using ZooWorld.Core.Animals;
using ZooWorld.Core.FoodChain;

namespace ZooWorld.Core.Tests.Fakes
{
    public sealed class FixedTieBreaker : ITieBreaker
    {
        private readonly bool _firstWins;

        public FixedTieBreaker(bool firstWins) => _firstWins = firstWins;

        public int Calls { get; private set; }

        public bool FirstWins(AnimalProfile first, AnimalProfile second)
        {
            Calls++;
            return _firstWins;
        }
    }
}
