using NUnit.Framework;
using ZooWorld.Core.Animals;
using ZooWorld.Game.Configs;
using ZooWorld.Game.Tests.Fakes;

namespace ZooWorld.Game.Tests
{
    public class AnimalProfileMapperTests
    {
        private AnimalTag _animal;
        private AnimalTag _frogTag;
        private AnimalConfig _frog;
        private AnimalConfig _snake;

        [SetUp]
        public void SetUp()
        {
            _animal = TestAssets.Tag("Animal");
            _frogTag = TestAssets.Tag("Frog");
            _frog = TestAssets.Animal("Frog", new[] { _animal, _frogTag }, new AnimalTag[0]);
            _snake = TestAssets.Animal("Snake", new[] { _animal }, new[] { _animal });
        }

        [Test]
        public void SameTagAsset_MapsToSameId()
        {
            var mapper = new AnimalProfileMapper();

            Assert.That(mapper.GetTagId(_animal), Is.EqualTo(mapper.GetTagId(_animal)));
            Assert.That(mapper.GetTagId(_animal), Is.Not.EqualTo(mapper.GetTagId(_frogTag)));
        }

        [Test]
        public void Profiles_CarryTheConfiguredFoodChain()
        {
            var mapper = new AnimalProfileMapper();
            var frog = mapper.GetProfile(_frog);
            var snake = mapper.GetProfile(_snake);

            Assert.That(snake.CanEat(frog), Is.True);
            Assert.That(frog.CanEat(snake), Is.False);
            Assert.That(frog.Role, Is.EqualTo(AnimalRole.Prey));
            Assert.That(snake.Role, Is.EqualTo(AnimalRole.Predator));
            Assert.That(frog.Name, Is.EqualTo("Frog"));
        }

        [Test]
        public void Profile_IsCachedPerConfig()
        {
            var mapper = new AnimalProfileMapper();

            Assert.That(mapper.GetProfile(_frog), Is.SameAs(mapper.GetProfile(_frog)));
        }

        [Test]
        public void IncompleteConfig_ReportsProblems()
        {
            Assert.That(_frog.Validate(), Has.Some.Contains("Prefab"));
            Assert.That(_frog.Validate(), Has.Some.Contains("Movement"));
        }
    }
}
