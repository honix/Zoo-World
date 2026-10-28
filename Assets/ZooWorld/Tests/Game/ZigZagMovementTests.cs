using NUnit.Framework;
using UnityEngine;
using ZooWorld.Core.Randomness;
using ZooWorld.Game.Movement;
using ZooWorld.Game.Tests.Fakes;

namespace ZooWorld.Game.Tests
{
    public class ZigZagMovementTests
    {
        private const float Step = 0.02f;

        [Test]
        public void SwingsToBothSidesOfTheCourse()
        {
            var movement = new ZigZagMovement(2f, 30f, 0.5f, new SeededRandom(1));
            movement.Steer(Vector3.forward);
            var body = new FakeBody();
            float minAngle = 0f, maxAngle = 0f;

            for (var i = 0; i < 100; i++)
            {
                movement.Tick(body, Step);
                var angle = Vector3.SignedAngle(Vector3.forward, movement.Heading, Vector3.up);
                minAngle = Mathf.Min(minAngle, angle);
                maxAngle = Mathf.Max(maxAngle, angle);
            }

            Assert.That(minAngle, Is.EqualTo(-30f).Within(0.01f));
            Assert.That(maxAngle, Is.EqualTo(30f).Within(0.01f));
        }

        [Test]
        public void MakesProgressAlongTheCourse()
        {
            var movement = new ZigZagMovement(2f, 30f, 0.5f, new SeededRandom(1));
            movement.Steer(Vector3.right);
            var body = new FakeBody();

            for (var i = 0; i < 100; i++)
            {
                movement.Tick(body, Step);
                body.Integrate(Step);
            }

            // Two seconds at 2 m/s, swinging 30 degrees: about 2 * 2 * cos(30) along the course.
            Assert.That(body.Position.x, Is.EqualTo(4f * Mathf.Cos(30f * Mathf.Deg2Rad)).Within(0.05f));
            Assert.That(Mathf.Abs(body.Position.z), Is.LessThan(0.6f));
        }

        [Test]
        public void MovesAtConstantSpeed()
        {
            var movement = new ZigZagMovement(2.5f, 45f, 0.3f, new SeededRandom(2));
            var body = new FakeBody();

            for (var i = 0; i < 50; i++)
            {
                movement.Tick(body, Step);
                Assert.That(body.Velocity.magnitude, Is.EqualTo(2.5f).Within(1e-4f));
            }
        }

        [Test]
        public void DefaultSettings_AreValidAndCreateTheMovement()
        {
            var settings = ScriptableObject.CreateInstance<ZigZagMovementSettings>();

            Assert.That(settings.Validate(), Is.Empty);
            Assert.That(settings.CreateMovement(new SeededRandom(0)), Is.TypeOf<ZigZagMovement>());
        }
    }
}
