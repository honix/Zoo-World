using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ZooWorld.Core.Animals;
using ZooWorld.Core.FoodChain;
using ZooWorld.Core.Tests.Fakes;

namespace ZooWorld.Core.Tests
{
    public class CollisionServiceTests
    {
        private AnimalRegistry _registry;
        private CollisionService _collisions;
        private List<Meal> _meals;
        private List<AnimalDeath> _deaths;

        [SetUp]
        public void SetUp()
        {
            _registry = new AnimalRegistry();
            _collisions = new CollisionService(_registry, new DietFoodChain(new FixedTieBreaker(firstWins: true)));
            _meals = new List<Meal>();
            _deaths = new List<AnimalDeath>();
            _collisions.Eaten += _meals.Add;
            _registry.Died += _deaths.Add;
        }

        [Test]
        public void PredatorEatsPrey_PreyDiesAndMealIsReported()
        {
            var snake = _registry.Register(Zoo.Snake());
            var frog = _registry.Register(Zoo.Frog());

            _collisions.Report(frog, snake);

            Assert.That(_registry.IsAlive(frog), Is.False);
            Assert.That(_registry.IsAlive(snake), Is.True);
            Assert.That(_meals, Is.EqualTo(new[] { new Meal(snake, frog) }));
        }

        [Test]
        public void PreyMeetsPrey_NobodyDies()
        {
            var a = _registry.Register(Zoo.Frog());
            var b = _registry.Register(Zoo.Frog());

            _collisions.Report(a, b);

            Assert.That(_deaths, Is.Empty);
            Assert.That(_meals, Is.Empty);
        }

        [Test]
        public void MirroredReport_IsProcessedOnce()
        {
            var a = _registry.Register(Zoo.Snake());
            var b = _registry.Register(Zoo.Snake());

            // Unity calls OnCollisionEnter on both colliders. With a naive
            // tie-breaker "first wins", each call alone would kill the other one.
            _collisions.Report(a, b);
            _collisions.Report(b, a);

            Assert.That(_deaths, Has.Count.EqualTo(1));
            Assert.That(_meals, Has.Count.EqualTo(1));
            Assert.That(_registry.IsAlive(a) ^ _registry.IsAlive(b), Is.True, "exactly one predator survives");
        }

        [Test]
        public void Outcome_DoesNotDependOnReportOrder()
        {
            // The engine may report the same contact as (a, b) or (b, a).
            // With a tie-breaker that always favours "first", both orders must agree.
            var survivors = new[] { false, true }.Select(reverse =>
            {
                var registry = new AnimalRegistry();
                var collisions = new CollisionService(registry, new DietFoodChain(new FixedTieBreaker(firstWins: true)));
                var a = registry.Register(Zoo.Snake());
                var b = registry.Register(Zoo.Snake());

                if (reverse) collisions.Report(b, a);
                else collisions.Report(a, b);

                return registry.IsAlive(a) ? a : b;
            }).ToArray();

            Assert.That(survivors[1], Is.EqualTo(survivors[0]));
        }

        [Test]
        public void DeadAnimal_CannotEatOrBeEaten()
        {
            var frog = _registry.Register(Zoo.Frog());
            var snake = _registry.Register(Zoo.Snake());
            var otherSnake = _registry.Register(Zoo.Snake());
            _collisions.Report(snake, frog);

            _collisions.Report(otherSnake, frog);

            Assert.That(_deaths, Has.Count.EqualTo(1));
            Assert.That(_registry.IsAlive(otherSnake), Is.True);
        }

        [Test]
        public void SelfCollision_IsIgnored()
        {
            var snake = _registry.Register(Zoo.Snake());

            _collisions.Report(snake, snake);

            Assert.That(_registry.IsAlive(snake), Is.True);
        }

        [Test]
        public void MealIsReported_AfterVictimIsDead()
        {
            var snake = _registry.Register(Zoo.Snake());
            var frog = _registry.Register(Zoo.Frog());
            var victimAliveDuringEvent = true;
            _collisions.Eaten += meal => victimAliveDuringEvent = _registry.IsAlive(meal.Victim);

            _collisions.Report(snake, frog);

            Assert.That(victimAliveDuringEvent, Is.False);
        }
    }
}
