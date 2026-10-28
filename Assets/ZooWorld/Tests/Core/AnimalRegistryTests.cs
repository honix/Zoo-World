using NUnit.Framework;
using ZooWorld.Core.Animals;
using ZooWorld.Core.Tests.Fakes;

namespace ZooWorld.Core.Tests
{
    public class AnimalRegistryTests
    {
        [Test]
        public void Register_IssuesUniqueIds()
        {
            var registry = new AnimalRegistry();

            var a = registry.Register(Zoo.Frog());
            var b = registry.Register(Zoo.Frog());

            Assert.That(a, Is.Not.EqualTo(b));
            Assert.That(registry.AliveCount, Is.EqualTo(2));
        }

        [Test]
        public void Kill_RemovesImmediatelyAndRaisesDiedOnce()
        {
            var registry = new AnimalRegistry();
            var frog = registry.Register(Zoo.Frog());
            var deaths = 0;
            registry.Died += _ => deaths++;

            Assert.That(registry.Kill(frog), Is.True);
            Assert.That(registry.Kill(frog), Is.False);

            Assert.That(registry.IsAlive(frog), Is.False);
            Assert.That(registry.AliveCount, Is.Zero);
            Assert.That(deaths, Is.EqualTo(1));
        }

        [Test]
        public void UnknownId_IsNotAlive()
        {
            Assert.That(new AnimalRegistry().IsAlive(new AnimalId(999)), Is.False);
        }
    }
}
