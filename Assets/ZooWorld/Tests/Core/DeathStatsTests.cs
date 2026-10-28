using NUnit.Framework;
using ZooWorld.Core.Animals;
using ZooWorld.Core.Stats;
using ZooWorld.Core.Tests.Fakes;

namespace ZooWorld.Core.Tests
{
    public class DeathStatsTests
    {
        [Test]
        public void CountsDeathsByRole()
        {
            var registry = new AnimalRegistry();
            using var stats = new DeathStats(registry);

            registry.Kill(registry.Register(Zoo.Frog()));
            registry.Kill(registry.Register(Zoo.Frog()));
            registry.Kill(registry.Register(Zoo.Snake()));

            Assert.That(stats.DeadPrey, Is.EqualTo(2));
            Assert.That(stats.DeadPredators, Is.EqualTo(1));
            Assert.That(stats.Total, Is.EqualTo(3));
        }

        [Test]
        public void RaisesChanged_OnEveryDeath()
        {
            var registry = new AnimalRegistry();
            using var stats = new DeathStats(registry);
            var changes = 0;
            stats.Changed += () => changes++;

            registry.Kill(registry.Register(Zoo.Frog()));
            registry.Kill(registry.Register(Zoo.Spider()));

            Assert.That(changes, Is.EqualTo(2));
        }

        [Test]
        public void Dispose_StopsCounting()
        {
            var registry = new AnimalRegistry();
            var stats = new DeathStats(registry);

            stats.Dispose();
            registry.Kill(registry.Register(Zoo.Frog()));

            Assert.That(stats.Total, Is.Zero);
        }
    }
}
