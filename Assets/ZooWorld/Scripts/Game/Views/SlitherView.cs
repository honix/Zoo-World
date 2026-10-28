using PrimeTween;
using UnityEngine;
using ZooWorld.Game.Animals;

namespace ZooWorld.Game.Views
{
    /// <summary>
    /// Makes a long body follow its head: each tail segment sits on the path the head
    /// has travelled, so turns flow down the body instead of swinging it like a stick.
    /// A travelling wave sways the segments sideways; the head bobs and shakes on bumps.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class SlitherView : MonoBehaviour
    {
        [SerializeField] private SlitherPreset _preset;
        [SerializeField] private Transform _head;
        [SerializeField] private Transform[] _segments = new Transform[0];

        private Animal _animal;
        private PathTrail _trail;
        private Vector3 _headRest;
        private float[] _distances;
        private float[] _heights;
        private float _phase;
        private Tween _headShake;

        private void Awake()
        {
            _animal = GetComponentInParent<Animal>();
            if (_head != null) _headRest = _head.localPosition;

            // Each segment keeps the distance behind the animal it was authored at.
            _distances = new float[_segments.Length];
            _heights = new float[_segments.Length];
            var longest = 0f;
            for (var i = 0; i < _segments.Length; i++)
            {
                var rest = _segments[i].localPosition;
                _distances[i] = Mathf.Max(0f, -rest.z);
                _heights[i] = rest.y;
                longest = Mathf.Max(longest, _distances[i]);
            }

            _trail = new PathTrail(_preset.TrailSpacing, longest + 1f);
            _animal.Spawned += OnSpawned;
            _animal.Redirected += OnRedirected;
        }

        private void OnDestroy()
        {
            if (_headShake.isAlive) _headShake.Stop();
            if (_animal == null) return;

            _animal.Spawned -= OnSpawned;
            _animal.Redirected -= OnRedirected;
        }

        private void OnSpawned()
        {
            _trail.Reset(_animal.transform.position, -_animal.transform.forward);
            if (_headShake.isAlive) _headShake.Stop();
            if (_head != null) _head.localRotation = Quaternion.identity;
        }

        private void OnRedirected(Redirect redirect)
        {
            if (redirect.Cause != RedirectCause.Bump || _head == null) return;

            if (_headShake.isAlive) _headShake.Stop();
            _head.localRotation = Quaternion.identity;
            _headShake = Tween.ShakeLocalRotation(_head, _preset.BumpHeadShake, _preset.BumpShakeDuration);
        }

        private void LateUpdate()
        {
            var root = _animal.transform.position;
            if (_animal.IsAlive)
            {
                _trail.Record(root);
                _phase += Time.deltaTime * _preset.Frequency * 2f * Mathf.PI;
            }

            // Follow the Visual's scale, so the tail pops in and poofs out with the rest of the body.
            var scale = transform.lossyScale.x;
            var origin = transform.position;
            var last = Mathf.Max(1, _segments.Length - 1);

            for (var i = 0; i < _segments.Length; i++)
            {
                var point = _trail.Sample(root, _distances[i], out var forward);
                var side = Vector3.Cross(Vector3.up, forward);
                var swing = _preset.Amplitude * _preset.AmplitudeAlongTail.Evaluate((float)i / last);
                var offset = point - root + side * (Mathf.Sin(_phase - i * _preset.PhasePerSegment) * swing) + Vector3.up * _heights[i];

                _segments[i].SetPositionAndRotation(origin + offset * scale, Quaternion.LookRotation(forward));
            }

            if (_head != null && _animal.IsAlive)
            {
                _head.localPosition = _headRest + Vector3.up * (Mathf.Abs(Mathf.Sin(_phase)) * _preset.HeadBob);
            }
        }
    }
}
