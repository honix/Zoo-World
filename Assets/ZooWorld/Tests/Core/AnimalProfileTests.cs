using NUnit.Framework;
using ZooWorld.Core.Animals;
using ZooWorld.Core.Tests.Fakes;

namespace ZooWorld.Core.Tests
{
    public class AnimalProfileTests
    {
        [Test]
        public void Snake_CanEatFrog()
        {
            Assert.That(Zoo.Snake().CanEat(Zoo.Frog()), Is.True);
        }

        [Test]
        public void Snake_CanEatAnotherSnake()
        {
            Assert.That(Zoo.Snake().CanEat(Zoo.Snake()), Is.True);
        }

        [Test]
        public void Frog_CanEatNothing()
        {
            var frog = Zoo.Frog();

            Assert.That(frog.CanEat(Zoo.Frog()), Is.False);
            Assert.That(frog.CanEat(Zoo.Snake()), Is.False);
        }

        [Test]
        public void Spider_EatsFrogsButNotSnakes()
        {
            var spider = Zoo.Spider();

            Assert.That(spider.CanEat(Zoo.Frog()), Is.True);
            Assert.That(spider.CanEat(Zoo.Snake()), Is.False);
        }

        [Test]
        public void Role_IsDerivedFromDiet()
        {
            Assert.That(Zoo.Frog().Role, Is.EqualTo(AnimalRole.Prey));
            Assert.That(Zoo.Snake().Role, Is.EqualTo(AnimalRole.Predator));
            Assert.That(Zoo.Spider().Role, Is.EqualTo(AnimalRole.Predator));
        }

        [Test]
        public void DuplicateTags_AreCollapsed()
        {
            var tag = new TagId(7);
            var profile = new AnimalProfile("Twice", new[] { tag, tag }, new[] { tag, tag });

            Assert.That(profile.Tags, Is.EqualTo(new[] { tag }));
            Assert.That(profile.Diet, Is.EqualTo(new[] { tag }));
        }

        [Test]
        public void TagName_IsForDisplayOnly()
        {
            var named = new TagId(7, "Reptile");

            Assert.That(named, Is.EqualTo(new TagId(7)));
            Assert.That(named.ToString(), Is.EqualTo("Reptile"));
            Assert.That(new TagId(7).ToString(), Is.EqualTo("Tag#7"));
        }
    }
}
