using UnityEngine;

namespace ZooWorld.Game.Views
{
    /// <summary>A travelling side-to-side wave along a tail, plus a small head bob.</summary>
    [CreateAssetMenu(menuName = "Zoo World/Views/Slither Preset", fileName = "SlitherPreset")]
    public sealed class SlitherPreset : ScriptableObject
    {
        [Tooltip("Sideways swing at the tip of the tail, in meters.")]
        [SerializeField, Min(0f)] private float _amplitude = 0.16f;

        [Tooltip("Waves per second.")]
        [SerializeField, Min(0f)] private float _frequency = 1.6f;

        [Tooltip("Phase shift between neighbouring segments, in radians.")]
        [SerializeField] private float _phasePerSegment = 0.7f;

        [Tooltip("How the swing grows from the first segment (0) to the tip (1).")]
        [SerializeField] private AnimationCurve _amplitudeAlongTail = AnimationCurve.Linear(0f, 0.25f, 1f, 1f);

        [Tooltip("Head bob height, in meters.")]
        [SerializeField, Min(0f)] private float _headBob = 0.03f;

        public float Amplitude => _amplitude;
        public float Frequency => _frequency;
        public float PhasePerSegment => _phasePerSegment;
        public AnimationCurve AmplitudeAlongTail => _amplitudeAlongTail;
        public float HeadBob => _headBob;
    }
}
