using NUnit.Framework;
using UnityEngine;
using ZooWorld.Game.Views;

namespace ZooWorld.Game.Tests
{
    public class PathTrailTests
    {
        private const float Tolerance = 0.02f;

        [Test]
        public void AfterReset_TheTailLiesStraightBehind()
        {
            var trail = new PathTrail(0.1f, 2f);
            trail.Reset(new Vector3(1f, 0f, 1f), Vector3.back);

            var point = trail.Sample(new Vector3(1f, 0f, 1f), 1.5f, out var forward);

            AssertNear(point, new Vector3(1f, 0f, -0.5f));
            AssertNear(forward, Vector3.forward);
        }

        [Test]
        public void StraightRun_IsSampledByDistance()
        {
            var trail = new PathTrail(0.1f, 3f);
            var head = Vector3.zero;
            trail.Reset(head, Vector3.left);

            for (var i = 1; i <= 50; i++) trail.Record(head = new Vector3(i * 0.1f, 0f, 0f));

            AssertNear(trail.Sample(head, 1f, out var forward), new Vector3(4f, 0f, 0f));
            AssertNear(forward, Vector3.right);
        }

        [Test]
        public void Corner_TheTailBendsAlongThePath()
        {
            // Three meters right, then three meters up: an L-shaped path.
            // Recording finer than the steps keeps every step, so the corner is exact.
            var trail = new PathTrail(0.05f, 6f);
            var head = Vector3.zero;
            trail.Reset(head, Vector3.left);
            for (var i = 1; i <= 30; i++) trail.Record(head = new Vector3(i * 0.1f, 0f, 0f));
            for (var i = 1; i <= 30; i++) trail.Record(head = new Vector3(3f, 0f, i * 0.1f));

            var onSecondLeg = trail.Sample(head, 2f, out var upward);
            var onFirstLeg = trail.Sample(head, 4f, out var rightward);

            AssertNear(onSecondLeg, new Vector3(3f, 0f, 1f));
            AssertNear(upward, Vector3.forward);
            AssertNear(onFirstLeg, new Vector3(2f, 0f, 0f));
            AssertNear(rightward, Vector3.right);
        }

        [Test]
        public void BeyondTheHistory_ItContinuesStraight()
        {
            var trail = new PathTrail(0.1f, 1f);
            var head = Vector3.zero;
            trail.Reset(head, Vector3.back);
            for (var i = 1; i <= 100; i++) trail.Record(head = new Vector3(0f, 0f, i * 0.1f));

            var far = trail.Sample(head, 5f, out var forward);

            AssertNear(far, new Vector3(0f, 0f, 5f));
            AssertNear(forward, Vector3.forward);
        }

        [Test]
        public void History_StaysBounded()
        {
            var trail = new PathTrail(0.1f, 2f);
            trail.Reset(Vector3.zero, Vector3.back);
            var countAfterReset = trail.Count;

            for (var i = 1; i <= 2000; i++) trail.Record(new Vector3(i * 0.1f, 0f, 0f));

            Assert.That(trail.Count, Is.LessThanOrEqualTo(countAfterReset + 1));
        }

        [Test]
        public void TinyMoves_AreNotRecorded()
        {
            var trail = new PathTrail(0.1f, 2f);
            trail.Reset(Vector3.zero, Vector3.back);
            var before = trail.Count;

            trail.Record(new Vector3(0.01f, 0f, 0f));

            Assert.That(trail.Count, Is.EqualTo(before));
        }

        private static void AssertNear(Vector3 actual, Vector3 expected) =>
            Assert.That(Vector3.Distance(actual, expected), Is.LessThan(Tolerance), $"{actual} vs {expected}");
    }
}
