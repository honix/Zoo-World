using UnityEngine;
using ZooWorld.Core.Randomness;
using ZooWorld.Game.Movement;

namespace ZooWorld.Game.World
{
    /// <summary>
    /// Brings animals that left the play area back: when an animal is outside and
    /// still heading away, it turns towards a random point in the middle of the area.
    /// </summary>
    public sealed class OffScreenSteering
    {
        /// <summary>Share of the area, around its center, that return targets are picked from.</summary>
        private const float TargetZone = 0.5f;

        private readonly IRandom _random;

        public OffScreenSteering(IRandom random) => _random = random;

        public bool TrySteer(GroundRect area, Vector3 position, Vector3 heading, out Vector3 newHeading)
        {
            newHeading = heading;
            if (!IsLeaving(area, position, heading)) return false;

            var target = TargetArea(area).RandomPoint(_random);
            newHeading = Planar.Direction(target - position, -heading);
            return true;
        }

        /// <summary>Outside the area on some side, and not moving back across that side.</summary>
        public static bool IsLeaving(GroundRect area, Vector3 position, Vector3 heading) =>
            (position.x < area.MinX && heading.x <= 0f) ||
            (position.x > area.MaxX && heading.x >= 0f) ||
            (position.z < area.MinZ && heading.z <= 0f) ||
            (position.z > area.MaxZ && heading.z >= 0f);

        private static GroundRect TargetArea(GroundRect area)
        {
            var shrink = 0.5f * (1f - TargetZone);
            return area.Inset(Mathf.Min(area.Width, area.Depth) * shrink);
        }
    }
}
