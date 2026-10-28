using UnityEngine;
using ZooWorld.Game.Animals;

namespace ZooWorld.Game.Views
{
    /// <summary>Sways tail segments in a travelling wave while the animal is alive.</summary>
    [DisallowMultipleComponent]
    public sealed class SlitherView : MonoBehaviour
    {
        [SerializeField] private SlitherPreset _preset;
        [SerializeField] private Transform _head;
        [SerializeField] private Transform[] _segments = new Transform[0];

        private Animal _animal;
        private Vector3 _headRest;
        private Vector3[] _segmentRest;
        private float _phase;

        private void Awake()
        {
            _animal = GetComponentInParent<Animal>();
            if (_head != null) _headRest = _head.localPosition;

            _segmentRest = new Vector3[_segments.Length];
            for (var i = 0; i < _segments.Length; i++) _segmentRest[i] = _segments[i].localPosition;
        }

        private void LateUpdate()
        {
            if (!_animal.IsAlive) return;

            _phase += Time.deltaTime * _preset.Frequency * 2f * Mathf.PI;

            var last = Mathf.Max(1, _segments.Length - 1);
            for (var i = 0; i < _segments.Length; i++)
            {
                var swing = _preset.Amplitude * _preset.AmplitudeAlongTail.Evaluate((float)i / last);
                var offset = Mathf.Sin(_phase - i * _preset.PhasePerSegment) * swing;
                _segments[i].localPosition = _segmentRest[i] + Vector3.right * offset;
            }

            if (_head != null)
            {
                _head.localPosition = _headRest + Vector3.up * (Mathf.Abs(Mathf.Sin(_phase)) * _preset.HeadBob);
            }
        }
    }
}
