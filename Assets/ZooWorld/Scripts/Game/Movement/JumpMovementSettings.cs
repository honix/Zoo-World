using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Core.Randomness;

namespace ZooWorld.Game.Movement
{
    [CreateAssetMenu(menuName = "Zoo World/Movement/Jump", fileName = "JumpMovement")]
    public sealed class JumpMovementSettings : MovementSettings
    {
        [Tooltip("Seconds from the start of one jump to the start of the next.")]
        [SerializeField, Min(0f)] private float _interval = 1.2f;

        [Tooltip("Ground distance covered by one jump, in meters.")]
        [SerializeField, Min(0f)] private float _distance = 2f;

        [Tooltip("How long one jump lasts, in seconds. Must be shorter than the interval.")]
        [SerializeField, Min(0f)] private float _duration = 0.35f;

        [Tooltip("Each jump turns randomly by up to this angle. 180 means any direction.")]
        [SerializeField, Range(0f, 180f)] private float _maxTurnDegrees = 120f;

        public float Interval => _interval;
        public float Distance => _distance;
        public float Duration => _duration;

        public override IMovement CreateMovement(IRandom random) =>
            new JumpMovement(_interval, _distance, _duration, _maxTurnDegrees, random);

        public override IEnumerable<string> Validate()
        {
            if (_interval <= 0f) yield return "Interval must be positive.";
            if (_distance <= 0f) yield return "Distance must be positive.";
            if (_duration <= 0f) yield return "Duration must be positive.";
            if (_duration >= _interval) yield return "Duration must be shorter than the interval.";
        }
    }
}
