using System.Linq;
using NUnit.Framework;
using ZooWorld.Core.FoodChain;
using ZooWorld.Core.Randomness;
using ZooWorld.Core.Tests.Fakes;

namespace ZooWorld.Core.Tests
{
    public class RandomnessTests
    {
        [Test]
        public void SameSeed_ProducesSameSequence()
        {
            var a = new SeededRandom(42);
            var b = new SeededRandom(42);

            var first = Enumerable.Range(0, 20).Select(_ => a.NextDouble()).ToArray();
            var second = Enumerable.Range(0, 20).Select(_ => b.NextDouble()).ToArray();

            Assert.That(second, Is.EqualTo(first));
        }

        [Test]
        public void Streams_AreReproducibleFromMasterSeed()
        {
            var a = new RandomStreams(123);
            var b = new RandomStreams(123);

            Assert.That(b.Spawning.NextDouble(), Is.EqualTo(a.Spawning.NextDouble()));
            Assert.That(b.Movement.NextDouble(), Is.EqualTo(a.Movement.NextDouble()));
            Assert.That(b.TieBreak.NextDouble(), Is.EqualTo(a.TieBreak.NextDouble()));
        }

        [Test]
        public void DrawingFromOneStream_DoesNotShiftAnother()
        {
            var quiet = new RandomStreams(7);
            var noisy = new RandomStreams(7);

            for (var i = 0; i < 100; i++) noisy.Movement.NextDouble();

            Assert.That(noisy.Spawning.NextDouble(), Is.EqualTo(quiet.Spawning.NextDouble()));
        }

        [Test]
        public void Streams_HaveDistinctSeeds()
        {
            var seeds = new[] { 1, 2, 3 }.Select(i => RandomStreams.DeriveSeed(0, i)).ToArray();

            Assert.That(seeds, Is.Unique);
        }

        [Test]
        public void Range_MapsUnitIntervalOntoBounds()
        {
            var random = new ScriptedRandom(0d, 0.5d);

            Assert.That(random.Range(1d, 2d), Is.EqualTo(1d));
            Assert.That(random.Range(1d, 2d), Is.EqualTo(1.5d));
        }

        [Test]
        public void CoinFlip_IsRoughlyFair()
        {
            var tieBreaker = new CoinFlipTieBreaker(new SeededRandom(2026));
            var frog = Zoo.Frog();

            var wins = Enumerable.Range(0, 10000).Count(_ => tieBreaker.FirstWins(frog, frog));

            Assert.That(wins, Is.InRange(4800, 5200));
        }
    }
}
