using NUnit.Framework;
using UnityEngine;
using ZooWorld.Core.Randomness;
using ZooWorld.Game.World;

namespace ZooWorld.Game.Tests
{
    public class PlayAreaTests
    {
        private Camera _camera;
        private RenderTexture _target;

        [SetUp]
        public void SetUp()
        {
            // A fixed-size target, so viewport rays don't depend on the Editor's Game view size.
            _target = new RenderTexture(2000, 1000, 0);
            _camera = new GameObject("Test Camera").AddComponent<Camera>();
            _camera.transform.SetPositionAndRotation(new Vector3(3f, 10f, -2f), Quaternion.Euler(90f, 0f, 0f));
            _camera.targetTexture = _target;
            _camera.fieldOfView = 60f;
            _camera.aspect = 2f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_camera.gameObject);
            Object.DestroyImmediate(_target);
        }

        [Test]
        public void TopDownCamera_SeesTheExpectedGround()
        {
            var visible = new CameraPlayArea(_camera, 0f, 0f).Visible();

            // Viewport rays go through the integer pixel rect, so allow centimeter rounding.
            const float tolerance = 0.01f;
            var halfDepth = 10f * Mathf.Tan(30f * Mathf.Deg2Rad);
            Assert.That(visible.Center.x, Is.EqualTo(3f).Within(tolerance));
            Assert.That(visible.Center.z, Is.EqualTo(-2f).Within(tolerance));
            Assert.That(visible.Depth, Is.EqualTo(2f * halfDepth).Within(tolerance));
            Assert.That(visible.Width, Is.EqualTo(4f * halfDepth).Within(tolerance));
        }

        [Test]
        public void Margins_ShrinkTheBounds()
        {
            var area = new CameraPlayArea(_camera, screenMargin: 0.5f, spawnMargin: 1.5f);
            var visible = area.Visible();

            Assert.That(area.Bounds.Width, Is.EqualTo(visible.Width - 1f).Within(1e-3f));
            Assert.That(area.SpawnBounds.Depth, Is.EqualTo(visible.Depth - 3f).Within(1e-3f));
        }

        [Test]
        public void Inset_NeverInvertsTheRect()
        {
            var rect = new GroundRect(0f, 0f, 2f, 1f).Inset(5f);

            Assert.That(rect.Width, Is.Zero);
            Assert.That(rect.Depth, Is.Zero);
            Assert.That(rect.Center, Is.EqualTo(new Vector3(1f, 0f, 0.5f)));
        }

        [Test]
        public void RandomPoint_IsInside()
        {
            var rect = new GroundRect(-4f, 1f, 4f, 3f);
            var random = new SeededRandom(8);

            for (var i = 0; i < 100; i++) Assert.That(rect.Contains(rect.RandomPoint(random)), Is.True);
        }
    }
}
