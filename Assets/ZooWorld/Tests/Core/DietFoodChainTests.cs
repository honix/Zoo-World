using NUnit.Framework;
using ZooWorld.Core.FoodChain;
using ZooWorld.Core.Tests.Fakes;

namespace ZooWorld.Core.Tests
{
    public class DietFoodChainTests
    {
        [Test]
        public void PreyMeetsPrey_Bounces()
        {
            var foodChain = new DietFoodChain(new FixedTieBreaker(true));

            Assert.That(foodChain.Resolve(Zoo.Frog(), Zoo.Frog()), Is.EqualTo(CollisionOutcome.Bounce));
        }

        [Test]
        public void PredatorMeetsPrey_PredatorEats_InEitherOrder()
        {
            var foodChain = new DietFoodChain(new FixedTieBreaker(true));

            Assert.That(foodChain.Resolve(Zoo.Snake(), Zoo.Frog()), Is.EqualTo(CollisionOutcome.FirstEatsSecond));
            Assert.That(foodChain.Resolve(Zoo.Frog(), Zoo.Snake()), Is.EqualTo(CollisionOutcome.SecondEatsFirst));
        }

        [TestCase(true, CollisionOutcome.FirstEatsSecond)]
        [TestCase(false, CollisionOutcome.SecondEatsFirst)]
        public void PredatorMeetsPredator_TieBreakerPicksTheSurvivor(bool firstWins, CollisionOutcome expected)
        {
            var tieBreaker = new FixedTieBreaker(firstWins);
            var foodChain = new DietFoodChain(tieBreaker);

            Assert.That(foodChain.Resolve(Zoo.Snake(), Zoo.Snake()), Is.EqualTo(expected));
            Assert.That(tieBreaker.Calls, Is.EqualTo(1));
        }

        [Test]
        public void OneSidedDiet_DoesNotConsultTieBreaker()
        {
            var tieBreaker = new FixedTieBreaker(true);
            var foodChain = new DietFoodChain(tieBreaker);

            // A snake eats spiders, a spider doesn't eat snakes: no tie to break.
            Assert.That(foodChain.Resolve(Zoo.Spider(), Zoo.Snake()), Is.EqualTo(CollisionOutcome.SecondEatsFirst));
            Assert.That(tieBreaker.Calls, Is.Zero);
        }

        [Test]
        public void PredatorsWithDisjointDiets_Bounce()
        {
            var foodChain = new DietFoodChain(new FixedTieBreaker(true));

            Assert.That(foodChain.Resolve(Zoo.Spider(), Zoo.Spider()), Is.EqualTo(CollisionOutcome.Bounce));
        }
    }
}
