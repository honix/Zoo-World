using System;
using NUnit.Framework;
using ZooWorld.Core.Randomness;
using ZooWorld.Core.Spawning;
using ZooWorld.Core.Tests.Fakes;

namespace ZooWorld.Core.Tests
{
    public class SpawnTimerTests
    {
        [Test]
        public void Intervals_StayWithinRange()
        {
            var timer = new SpawnTimer(new SeededRandom(1), 1f, 2f);

            for (var i = 0; i < 1000; i++)
            {
                Assert.That(timer.CurrentInterval, Is.InRange(1f, 2f));
                timer.Tick(timer.CurrentInterval);
            }
        }

        [Test]
        public void FiresOnlyWhenIntervalElapses()
        {
            var timer = new SpawnTimer(new ScriptedRandom(0.5), 1f, 2f); // interval = 1.5s

            Assert.That(timer.Tick(1.25f), Is.Zero);
            Assert.That(timer.Tick(0.25f), Is.EqualTo(1));
        }

        [Test]
        public void LargeStep_ReportsEveryDueSpawn()
        {
            var timer = new SpawnTimer(new ScriptedRandom(0d), 1f, 2f); // every 1s

            Assert.That(timer.Tick(3.5f), Is.EqualTo(3));
        }

        [Test]
        public void Overshoot_CarriesIntoNextInterval()
        {
            var timer = new SpawnTimer(new ScriptedRandom(0d), 1f, 2f); // every 1s

            Assert.That(timer.Tick(1.75f), Is.EqualTo(1));
            Assert.That(timer.Tick(0.25f), Is.EqualTo(1));
        }

        [Test]
        public void FixedInterval_IsAllowed()
        {
            var timer = new SpawnTimer(new SeededRandom(3), 1f, 1f);

            Assert.That(timer.CurrentInterval, Is.EqualTo(1f));
        }

        [TestCase(0f, 1f)]
        [TestCase(2f, 1f)]
        public void InvalidRange_Throws(float min, float max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SpawnTimer(new SeededRandom(0), min, max));
        }
    }
}
