using UnityEngine;
using ZooWorld.Game.Movement;

namespace ZooWorld.Game.Tests.Fakes
{
    /// <summary>A frictionless point body that integrates its own velocity.</summary>
    public sealed class FakeBody : IMovementBody
    {
        public Vector3 Position { get; set; }

        public Vector3 Velocity { get; set; }

        public void Integrate(float deltaTime) => Position += Velocity * deltaTime;
    }
}
