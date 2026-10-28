using UnityEngine;
using ZooWorld.Core.Randomness;

namespace ZooWorld.Game.Movement
{
    /// <summary>Small helpers for directions on the ground (XZ) plane.</summary>
    public static class Planar
    {
        public static Vector3 Flatten(Vector3 vector) => new Vector3(vector.x, 0f, vector.z);

        /// <summary>Normalized ground direction, or <paramref name="fallback"/> if the vector is (almost) zero.</summary>
        public static Vector3 Direction(Vector3 vector, Vector3 fallback)
        {
            var flat = Flatten(vector);
            return flat.sqrMagnitude > 1e-6f ? flat.normalized : fallback;
        }

        public static Vector3 FromAngle(float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
        }

        public static Vector3 RandomDirection(IRandom random) => FromAngle((float)random.Range(0d, 360d));

        public static Vector3 Rotate(Vector3 direction, float degrees) =>
            Quaternion.AngleAxis(degrees, Vector3.up) * direction;
    }
}
