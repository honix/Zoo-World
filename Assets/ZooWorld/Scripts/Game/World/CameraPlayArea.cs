using UnityEngine;

namespace ZooWorld.Game.World
{
    /// <summary>
    /// The camera's view projected onto the ground plane, minus margins.
    /// Recomputed on every read, so it follows resolution and camera changes.
    /// </summary>
    public sealed class CameraPlayArea : IPlayArea
    {
        private static readonly Vector2[] ViewportCorners =
        {
            new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f)
        };

        private readonly Camera _camera;
        private readonly float _screenMargin;
        private readonly float _spawnMargin;
        private readonly Plane _ground;

        public CameraPlayArea(Camera camera, float screenMargin, float spawnMargin, float groundHeight = 0f)
        {
            _camera = camera;
            _screenMargin = screenMargin;
            _spawnMargin = spawnMargin;
            _ground = new Plane(Vector3.up, new Vector3(0f, groundHeight, 0f));
        }

        public GroundRect Bounds => Visible().Inset(_screenMargin);

        public GroundRect SpawnBounds => Visible().Inset(_spawnMargin);

        /// <summary>
        /// Bounding box of the four viewport corners on the ground. Exact for a camera
        /// looking straight down, which is how the zoo is framed.
        /// </summary>
        public GroundRect Visible()
        {
            float minX = float.MaxValue, minZ = float.MaxValue, maxX = float.MinValue, maxZ = float.MinValue;

            foreach (var corner in ViewportCorners)
            {
                var ray = _camera.ViewportPointToRay(corner);
                if (!_ground.Raycast(ray, out var distance)) continue;

                var point = ray.GetPoint(distance);
                minX = Mathf.Min(minX, point.x);
                minZ = Mathf.Min(minZ, point.z);
                maxX = Mathf.Max(maxX, point.x);
                maxZ = Mathf.Max(maxZ, point.z);
            }

            return minX <= maxX ? new GroundRect(minX, minZ, maxX, maxZ) : default;
        }
    }
}
