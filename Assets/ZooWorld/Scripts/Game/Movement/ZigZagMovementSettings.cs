using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Core.Randomness;

namespace ZooWorld.Game.Movement
{
    /// <summary>
    /// Scuttles forward in a zig-zag: every leg it swings to the other side of its course.
    /// Added as a single file to show what a new movement style costs.
    /// </summary>
    [CreateAssetMenu(menuName = "Zoo World/Movement/Zig-Zag", fileName = "ZigZagMovement")]
    public sealed class ZigZagMovementSettings : MovementSettings
    {
        [Tooltip("Meters per second.")]
        [SerializeField, Min(0f)] private float _speed = 2.2f;

        [Tooltip("How far each leg swings off the course, in degrees.")]
        [SerializeField, Range(0f, 80f)] private float _swingDegrees = 35f;

        [Tooltip("Seconds per leg before swinging to the other side.")]
        [SerializeField, Min(0f)] private float _legDuration = 0.45f;

        public override IMovement CreateMovement(IRandom random) =>
            new ZigZagMovement(_speed, _swingDegrees, _legDuration, random);

        public override IEnumerable<string> Validate()
        {
            if (_speed <= 0f) yield return "Speed must be positive.";
            if (_legDuration <= 0f) yield return "Leg duration must be positive.";
        }
    }

    public sealed class ZigZagMovement : IMovement
    {
        private readonly float _speed;
        private readonly float _swingDegrees;
        private readonly float _legDuration;

        private Vector3 _course;
        private float _side = 1f;
        private float _legTime;

        public ZigZagMovement(float speed, float swingDegrees, float legDuration, IRandom random)
        {
            _speed = speed;
            _swingDegrees = swingDegrees;
            _legDuration = legDuration;
            _course = Planar.RandomDirection(random);
            _legTime = (float)random.Range(0d, legDuration);
        }

        public Vector3 Heading => Planar.Rotate(_course, _side * _swingDegrees);

        public void Tick(IMovementBody body, float deltaTime)
        {
            _legTime += deltaTime;
            if (_legTime >= _legDuration)
            {
                _legTime -= _legDuration;
                _side = -_side;
            }

            body.Velocity = Heading * _speed;
        }

        public void Steer(Vector3 heading) => _course = Planar.Direction(heading, _course);

        public void OnBumped(IMovementBody body) => _course = Planar.Direction(body.Velocity, _course);
    }
}
