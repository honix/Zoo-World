using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using ZooWorld.Core.Animals;
using ZooWorld.Core.Randomness;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Movement;

namespace ZooWorld.Game.Tests
{
    public class AnimalRedirectTests
    {
        private Animal _animal;
        private List<Redirect> _redirects;

        [SetUp]
        public void SetUp()
        {
            _animal = new GameObject("Animal").AddComponent<Animal>();
            var movement = new LinearMovement(1f, new SeededRandom(1));
            movement.Steer(Vector3.forward);
            _animal.Activate(new AnimalId(1), null, movement, null, Vector3.zero);
            _redirects = new List<Redirect>();
            _animal.Redirected += _redirects.Add;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_animal.gameObject);

        [Test]
        public void Steer_ReportsAScreenEdgeTurnWithItsAngle()
        {
            _animal.Steer(Vector3.right);

            Assert.That(_redirects, Has.Count.EqualTo(1));
            Assert.That(_redirects[0].Cause, Is.EqualTo(RedirectCause.ScreenEdge));
            Assert.That(_redirects[0].Angle, Is.EqualTo(90f).Within(0.01f), "positive is a turn to the right");
            Assert.That(_animal.Movement.Heading, Is.EqualTo(Vector3.right));
        }

        [Test]
        public void Bump_FollowsTheBounceAndReportsIt()
        {
            _animal.Velocity = Vector3.left;

            _animal.Bump();

            Assert.That(_redirects, Has.Count.EqualTo(1));
            Assert.That(_redirects[0].Cause, Is.EqualTo(RedirectCause.Bump));
            Assert.That(_redirects[0].Angle, Is.EqualTo(-90f).Within(0.01f));
            Assert.That(_animal.Movement.Heading, Is.EqualTo(Vector3.left));
        }
    }
}
