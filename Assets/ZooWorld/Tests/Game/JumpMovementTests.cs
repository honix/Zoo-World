using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ZooWorld.Core.Randomness;
using ZooWorld.Game.Movement;
using ZooWorld.Game.Tests.Fakes;

namespace ZooWorld.Game.Tests
{
    public class JumpMovementTests
    {
        private const float Step = 0.02f;
        private const float Interval = 1f;
        private const float Distance = 2f;
        private const float Duration = 0.33f; // deliberately not a multiple of the step

        private JumpMovement _movement;
        private FakeBody _body;
        private float _time;

        [SetUp]
        public void SetUp()
        {
            _movement = new JumpMovement(Interval, Distance, Duration, 90f, new SeededRandom(4));
            _body = new FakeBody();
            _time = 0f;
        }

        [Test]
        public void JumpsOncePerInterval()
        {
            var jumpTimes = new List<float>();
            _movement.Jumped += () => jumpTimes.Add(_time);

            Run(seconds: 10.5f);

            Assert.That(jumpTimes, Has.Count.InRange(10, 11));
            for (var i = 1; i < jumpTimes.Count; i++)
            {
                Assert.That(jumpTimes[i] - jumpTimes[i - 1], Is.EqualTo(Interval).Within(Step));
            }
        }

        [Test]
        public void EachJump_CoversTheFixedDistance()
        {
            var takeOff = Vector3.zero;
            var distances = new List<float>();
            _movement.Jumped += () => takeOff = _body.Position;
            _movement.Landed += () => distances.Add(Vector3.Distance(takeOff, _body.Position));

            Run(seconds: 5f);

            Assert.That(distances, Is.Not.Empty);
            Assert.That(distances, Has.All.EqualTo(Distance).Within(1e-4f));
        }

        [Test]
        public void StandsStill_BetweenJumps()
        {
            _movement.Landed += () => Assert.That(_body.Velocity, Is.EqualTo(Vector3.zero));

            Run(seconds: 3f);
        }

        [Test]
        public void Bump_EndsTheJump()
        {
            RunUntil(() => _movement.IsJumping);
            var landed = false;
            _movement.Landed += () => landed = true;

            _movement.OnBumped(_body);

            Assert.That(_movement.IsJumping, Is.False);
            Assert.That(landed, Is.True);
        }

        [Test]
        public void Steer_SetsTheDirectionOfTheNextJump()
        {
            RunUntil(() => !_movement.IsJumping);

            _movement.Steer(Vector3.right);
            RunUntil(() => _movement.IsJumping);

            Assert.That(_movement.Heading, Is.EqualTo(Vector3.right));
            Assert.That(_body.Velocity.normalized, Is.EqualTo(Vector3.right));
        }

        private void Run(float seconds)
        {
            var steps = Mathf.RoundToInt(seconds / Step);
            for (var i = 0; i < steps; i++) StepOnce();
        }

        private void RunUntil(System.Func<bool> condition)
        {
            for (var i = 0; i < 10000 && !condition(); i++) StepOnce();
            Assert.That(condition(), Is.True, "condition was never reached");
        }

        private void StepOnce()
        {
            _time += Step;
            _movement.Tick(_body, Step);
            _body.Integrate(Step);
        }
    }
}
