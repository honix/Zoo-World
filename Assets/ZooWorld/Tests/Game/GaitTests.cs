using NUnit.Framework;
using ZooWorld.Game.Views;

namespace ZooWorld.Game.Tests
{
    public class GaitTests
    {
        [Test]
        public void LegsHalfACycleApart_Alternate()
        {
            for (var cycle = 0f; cycle < 2f; cycle += 0.05f)
            {
                Gait.Evaluate(cycle, 0f, out var swingA, out var liftA);
                Gait.Evaluate(cycle, 0.5f, out var swingB, out var liftB);

                Assert.That(swingB, Is.EqualTo(-swingA).Within(1e-4f), "one leg forward, the other back");
                Assert.That(liftA > 0.01f && liftB > 0.01f, Is.False, "never both feet up at once");
            }
        }

        [Test]
        public void Foot_LiftsOnlyWhileMovingForward()
        {
            const float step = 0.01f;
            for (var cycle = 0f; cycle < 1f; cycle += step)
            {
                Gait.Evaluate(cycle, 0f, out var swing, out var lift);
                Gait.Evaluate(cycle + step, 0f, out var nextSwing, out _);

                if (lift > 0.05f) Assert.That(nextSwing, Is.GreaterThan(swing), $"lifted at cycle {cycle} while moving back");
            }
        }

        [Test]
        public void SwingAndLift_StayInRange()
        {
            for (var cycle = -3f; cycle < 3f; cycle += 0.037f)
            {
                Gait.Evaluate(cycle, 0.25f, out var swing, out var lift);

                Assert.That(swing, Is.InRange(-1f, 1f));
                Assert.That(lift, Is.InRange(0f, 1f));
            }
        }
    }
}
