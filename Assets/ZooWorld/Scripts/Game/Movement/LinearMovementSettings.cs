using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Core.Randomness;

namespace ZooWorld.Game.Movement
{
    [CreateAssetMenu(menuName = "Zoo World/Movement/Linear", fileName = "LinearMovement")]
    public sealed class LinearMovementSettings : MovementSettings
    {
        [Tooltip("Distance covered per second, in meters.")]
        [SerializeField, Min(0f)] private float _speed = 2f;

        public float Speed => _speed;

        public override IMovement CreateMovement(IRandom random) => new LinearMovement(_speed, random);

        public override IEnumerable<string> Validate()
        {
            if (_speed <= 0f) yield return "Speed must be positive.";
        }
    }
}
