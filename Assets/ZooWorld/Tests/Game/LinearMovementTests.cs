using NUnit.Framework;
using UnityEngine;
using ZooWorld.Core.Randomness;
using ZooWorld.Game.Movement;
using ZooWorld.Game.Tests.Fakes;

namespace ZooWorld.Game.Tests
{
    public class LinearMovementTests
    {
        private const float Step = 0.02f;

        [Test]
        public void CoversSpeedMetersPerSecond()
        {
            var movement = new LinearMovement(3f, new SeededRandom(1));
            var body = new FakeBody();

            for (var i = 0; i < 50; i++)
            {
                movement.Tick(body, Step);
                body.Integrate(Step);
            }

            Assert.That(body.Position.magnitude, Is.EqualTo(3f).Within(1e-3f));
        }

        [Test]
        public void Heading_IsNormalizedAndOnTheGround()
        {
            for (var seed = 0; seed < 20; seed++)
            {
                var heading = new LinearMovement(1f, new SeededRandom(seed)).Heading;

                Assert.That(heading.magnitude, Is.EqualTo(1f).Within(1e-5f));
                Assert.That(heading.y, Is.Zero);
            }
        }

        [Test]
        public void Steer_ChangesDirection()
        {
            var movement = new LinearMovement(2f, new SeededRandom(1));
            var body = new FakeBody();

            movement.Steer(new Vector3(0f, 5f, -10f));
            movement.Tick(body, Step);

            Assert.That(movement.Heading, Is.EqualTo(Vector3.back));
            Assert.That(body.Velocity, Is.EqualTo(Vector3.back * 2f));
        }

        [Test]
        public void SteerTowardsZero_KeepsHeading()
        {
            var movement = new LinearMovement(2f, new SeededRandom(1));
            var before = movement.Heading;

            movement.Steer(Vector3.zero);

            Assert.That(movement.Heading, Is.EqualTo(before));
        }

        [Test]
        public void Bump_ContinuesInTheBounceDirection()
        {
            var movement = new LinearMovement(2f, new SeededRandom(1));
            var body = new FakeBody { Velocity = new Vector3(-0.5f, 0f, 0f) };

            movement.OnBumped(body);

            Assert.That(movement.Heading, Is.EqualTo(Vector3.left));
        }
    }
}
