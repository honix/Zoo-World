using UnityEngine;
using ZooWorld.Core.Randomness;

namespace ZooWorld.Game.Movement
{
    /// <summary>Moves straight ahead at a constant speed (a fixed distance per second).</summary>
    public sealed class LinearMovement : IMovement
    {
        private readonly float _speed;

        public LinearMovement(float speed, IRandom random)
        {
            _speed = speed;
            Heading = Planar.RandomDirection(random);
        }

        public Vector3 Heading { get; private set; }

        public void Tick(IMovementBody body, float deltaTime) => body.Velocity = Heading * _speed;

        public void Steer(Vector3 heading) => Heading = Planar.Direction(heading, Heading);

        // Keep going wherever the bounce sent us.
        public void OnBumped(IMovementBody body) => Heading = Planar.Direction(body.Velocity, Heading);
    }
}
