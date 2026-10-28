using UnityEngine;
using ZooWorld.Core.Randomness;

namespace ZooWorld.Game.World
{
    /// <summary>An axis-aligned rectangle on the ground (XZ) plane.</summary>
    public readonly struct GroundRect
    {
        public GroundRect(float minX, float minZ, float maxX, float maxZ)
        {
            MinX = Mathf.Min(minX, maxX);
            MinZ = Mathf.Min(minZ, maxZ);
            MaxX = Mathf.Max(minX, maxX);
            MaxZ = Mathf.Max(minZ, maxZ);
        }

        public float MinX { get; }
        public float MinZ { get; }
        public float MaxX { get; }
        public float MaxZ { get; }

        public float Width => MaxX - MinX;
        public float Depth => MaxZ - MinZ;
        public Vector3 Center => new Vector3((MinX + MaxX) * 0.5f, 0f, (MinZ + MaxZ) * 0.5f);

        public bool Contains(Vector3 point) =>
            point.x >= MinX && point.x <= MaxX && point.z >= MinZ && point.z <= MaxZ;

        /// <summary>Shrinks every side by <paramref name="margin"/>, never past the center.</summary>
        public GroundRect Inset(float margin)
        {
            var x = Mathf.Min(margin, Width * 0.5f);
            var z = Mathf.Min(margin, Depth * 0.5f);
            return new GroundRect(MinX + x, MinZ + z, MaxX - x, MaxZ - z);
        }

        public Vector3 RandomPoint(IRandom random) => new Vector3(
            (float)random.Range(MinX, MaxX), 0f, (float)random.Range(MinZ, MaxZ));

        public override string ToString() => $"[{MinX:0.##}..{MaxX:0.##}] x [{MinZ:0.##}..{MaxZ:0.##}]";
    }
}
