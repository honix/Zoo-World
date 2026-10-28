using System;
using System.Linq;
using NUnit.Framework;
using ZooWorld.Core.Randomness;
using ZooWorld.Core.Spawning;

namespace ZooWorld.Core.Tests
{
    public class WeightedRandomSpawnStrategyTests
    {
        [Test]
        public void Picks_FollowTheWeights()
        {
            var strategy = new WeightedRandomSpawnStrategy<string>(new[]
            {
                new SpawnEntry<string>("frog", 3d),
                new SpawnEntry<string>("snake", 1d)
            }, new SeededRandom(11));

            var frogs = Enumerable.Range(0, 10000).Count(_ => strategy.Next() == "frog");

            Assert.That(frogs, Is.InRange(7300, 7700));
        }

        [Test]
        public void ZeroWeight_IsNeverPicked()
        {
            var strategy = new WeightedRandomSpawnStrategy<string>(new[]
            {
                new SpawnEntry<string>("frog", 1d),
                new SpawnEntry<string>("disabled", 0d)
            }, new SeededRandom(5));

            Assert.That(Enumerable.Range(0, 1000).Select(_ => strategy.Next()), Has.None.EqualTo("disabled"));
        }

        [Test]
        public void SingleEntry_IsAlwaysPicked()
        {
            var strategy = new WeightedRandomSpawnStrategy<string>(
                new[] { new SpawnEntry<string>("frog", 0.1d) }, new SeededRandom(5));

            Assert.That(Enumerable.Range(0, 100).Select(_ => strategy.Next()), Has.All.EqualTo("frog"));
        }

        [Test]
        public void TableWithoutPositiveWeights_Throws()
        {
            Assert.Throws<ArgumentException>(() => new WeightedRandomSpawnStrategy<string>(
                new[] { new SpawnEntry<string>("frog", 0d) }, new SeededRandom(0)));

            Assert.Throws<ArgumentException>(() => new WeightedRandomSpawnStrategy<string>(
                new SpawnEntry<string>[0], new SeededRandom(0)));
        }

        [Test]
        public void SameSeed_GivesSameSequence()
        {
            var entries = new[] { new SpawnEntry<int>(1, 1d), new SpawnEntry<int>(2, 1d), new SpawnEntry<int>(3, 1d) };
            var a = new WeightedRandomSpawnStrategy<int>(entries, new SeededRandom(99));
            var b = new WeightedRandomSpawnStrategy<int>(entries, new SeededRandom(99));

            var first = Enumerable.Range(0, 50).Select(_ => a.Next()).ToArray();
            var second = Enumerable.Range(0, 50).Select(_ => b.Next()).ToArray();

            Assert.That(second, Is.EqualTo(first));
        }
    }
}
