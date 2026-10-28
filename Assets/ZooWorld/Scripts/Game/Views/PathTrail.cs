using System.Collections.Generic;
using UnityEngine;
using ZooWorld.Game.Movement;

namespace ZooWorld.Game.Views
{
    /// <summary>
    /// The recent path of a moving point, sampled by distance travelled.
    /// A snake's tail uses it so every segment follows where the head has been.
    /// </summary>
    public sealed class PathTrail
    {
        private readonly float _spacing;
        private readonly int _capacity;
        private readonly List<Vector3> _points; // oldest first
        private Vector3 _tailDirection = Vector3.back;

        /// <param name="spacing">Minimum distance between recorded points.</param>
        /// <param name="length">How much path to keep behind the head.</param>
        public PathTrail(float spacing, float length)
        {
            _spacing = Mathf.Max(0.01f, spacing);
            _capacity = Mathf.CeilToInt(length / _spacing) + 2;
            _points = new List<Vector3>(_capacity + 1);
        }

        public int Count => _points.Count;

        /// <summary>Forgets the path and lays a straight one behind <paramref name="head"/>.</summary>
        public void Reset(Vector3 head, Vector3 backward)
        {
            _points.Clear();
            _tailDirection = Planar.Direction(backward, Vector3.back);
            for (var i = _capacity - 1; i >= 0; i--) _points.Add(head + _tailDirection * (i * _spacing));
        }

        public void Record(Vector3 head)
        {
            if (_points.Count > 0 && (head - _points[_points.Count - 1]).sqrMagnitude < _spacing * _spacing) return;

            _points.Add(head);
            while (_points.Count > _capacity) _points.RemoveAt(0);
        }

        /// <summary>
        /// The point <paramref name="distance"/> meters back along the path from <paramref name="head"/>,
        /// and the direction of travel there. Beyond the recorded path, it continues straight.
        /// </summary>
        public Vector3 Sample(Vector3 head, float distance, out Vector3 forward)
        {
            var current = head;
            var remaining = distance;
            forward = -_tailDirection;

            for (var i = _points.Count - 1; i >= 0; i--)
            {
                var next = _points[i];
                var segment = next - current;
                var length = segment.magnitude;
                if (length < 1e-5f) continue;

                forward = -segment / length;
                if (length >= remaining) return current + segment * (remaining / length);

                remaining -= length;
                current = next;
            }

            var beyond = _points.Count >= 2 ? Planar.Direction(_points[0] - _points[1], _tailDirection) : _tailDirection;
            forward = -beyond;
            return current + beyond * remaining;
        }
    }
}
