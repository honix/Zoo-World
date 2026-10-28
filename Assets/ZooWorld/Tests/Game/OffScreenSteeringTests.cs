using NUnit.Framework;
using UnityEngine;
using ZooWorld.Core.Randomness;
using ZooWorld.Game.World;

namespace ZooWorld.Game.Tests
{
    public class OffScreenSteeringTests
    {
        private static readonly GroundRect Area = new GroundRect(-10f, -5f, 10f, 5f);

        [Test]
        public void Inside_IsLeftAlone()
        {
            var steering = new OffScreenSteering(new SeededRandom(1));

            var steered = steering.TrySteer(Area, Vector3.zero, Vector3.right, out var heading);

            Assert.That(steered, Is.False);
            Assert.That(heading, Is.EqualTo(Vector3.right));
        }

        [Test]
        public void OutsideAndComingBack_IsLeftAlone()
        {
            var steering = new OffScreenSteering(new SeededRandom(1));

            Assert.That(steering.TrySteer(Area, new Vector3(12f, 0f, 0f), Vector3.left, out _), Is.False);
        }

        // Every side, heading straight out, along the edge, and diagonally out.
        [TestCase(12f, 0f, 1f, 0f)]
        [TestCase(-12f, 0f, -1f, 0f)]
        [TestCase(0f, 7f, 0f, 1f)]
        [TestCase(0f, -7f, 0f, -1f)]
        [TestCase(12f, 0f, 0f, 1f)]
        [TestCase(11f, 6f, 0.7f, 0.7f)]
        public void OutsideAndLeaving_TurnsBackInside(float x, float z, float headingX, float headingZ)
        {
            var steering = new OffScreenSteering(new SeededRandom(1));
            var position = new Vector3(x, 0f, z);
            var outward = new Vector3(headingX, 0f, headingZ).normalized;

            for (var attempt = 0; attempt < 50; attempt++)
            {
                Assert.That(steering.TrySteer(Area, position, outward, out var heading), Is.True);
                Assert.That(OffScreenSteering.IsLeaving(Area, position, heading), Is.False, $"still leaving with {heading}");
                Assert.That(heading.magnitude, Is.EqualTo(1f).Within(1e-5f));
                Assert.That(Vector3.Dot(heading, Area.Center - position), Is.Positive);
            }
        }

        [Test]
        public void SteeredAnimal_ReachesTheAreaAgain()
        {
            var steering = new OffScreenSteering(new SeededRandom(3));
            var position = new Vector3(15f, 0f, -8f);
            var heading = new Vector3(1f, 0f, -1f).normalized;

            steering.TrySteer(Area, position, heading, out heading);
            for (var i = 0; i < 200 && !Area.Contains(position); i++) position += heading * 0.1f;

            Assert.That(Area.Contains(position), Is.True);
        }
    }
}
