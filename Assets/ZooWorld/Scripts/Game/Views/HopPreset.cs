using UnityEngine;

namespace ZooWorld.Game.Views
{
    /// <summary>Hop arc and squash and stretch for jumping animals.</summary>
    [CreateAssetMenu(menuName = "Zoo World/Views/Hop Preset", fileName = "HopPreset")]
    public sealed class HopPreset : ScriptableObject
    {
        [Tooltip("Peak height of the hop, in meters. Purely visual: logic moves on the ground.")]
        [SerializeField, Min(0f)] private float _height = 0.7f;

        [Tooltip("Height over the jump, 0 to 1 and back.")]
        [SerializeField] private AnimationCurve _arc = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 3.5f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f, -3.5f, 0f));

        [Header("Squash and stretch")]
        [SerializeField] private Vector3 _takeOffStretch = new Vector3(0.8f, 1.3f, 0.85f);
        [SerializeField, Min(0.01f)] private float _takeOffDuration = 0.12f;
        [SerializeField] private Vector3 _landingSquash = new Vector3(1.3f, 0.7f, 1.2f);
        [SerializeField, Min(0.01f)] private float _landingDuration = 0.25f;

        public float Height => _height;
        public AnimationCurve Arc => _arc;
        public Vector3 TakeOffStretch => _takeOffStretch;
        public float TakeOffDuration => _takeOffDuration;
        public Vector3 LandingSquash => _landingSquash;
        public float LandingDuration => _landingDuration;
    }
}
