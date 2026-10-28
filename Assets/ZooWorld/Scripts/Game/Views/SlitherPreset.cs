using UnityEngine;

namespace ZooWorld.Game.Views
{
    /// <summary>
    /// A tail that follows the head's path with a travelling side-to-side wave,
    /// a small head bob, and a head shake when the animal bumps into something.
    /// </summary>
    [CreateAssetMenu(menuName = "Zoo World/Views/Slither Preset", fileName = "SlitherPreset")]
    public sealed class SlitherPreset : ScriptableObject
    {
        [Header("Path")]
        [Tooltip("How finely the head's path is recorded, in meters. Smaller is smoother.")]
        [SerializeField, Min(0.01f)] private float _trailSpacing = 0.06f;

        [Header("Wave")]
        [Tooltip("Sideways swing at the tip of the tail, in meters.")]
        [SerializeField, Min(0f)] private float _amplitude = 0.16f;

        [Tooltip("Waves per second.")]
        [SerializeField, Min(0f)] private float _frequency = 1.6f;

        [Tooltip("Phase shift between neighbouring segments, in radians.")]
        [SerializeField] private float _phasePerSegment = 0.7f;

        [Tooltip("How the swing grows from the first segment (0) to the tip (1).")]
        [SerializeField] private AnimationCurve _amplitudeAlongTail = AnimationCurve.Linear(0f, 0.25f, 1f, 1f);

        [Header("Head")]
        [Tooltip("Head bob height, in meters.")]
        [SerializeField, Min(0f)] private float _headBob = 0.03f;

        [Tooltip("Head shake when bumping into something, in degrees per axis.")]
        [SerializeField] private Vector3 _bumpHeadShake = new Vector3(0f, 35f, 0f);
        [SerializeField, Min(0.01f)] private float _bumpShakeDuration = 0.4f;

        public float TrailSpacing => _trailSpacing;
        public float Amplitude => _amplitude;
        public float Frequency => _frequency;
        public float PhasePerSegment => _phasePerSegment;
        public AnimationCurve AmplitudeAlongTail => _amplitudeAlongTail;
        public float HeadBob => _headBob;
        public Vector3 BumpHeadShake => _bumpHeadShake;
        public float BumpShakeDuration => _bumpShakeDuration;
    }
}
