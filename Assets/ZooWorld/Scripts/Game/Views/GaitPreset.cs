using UnityEngine;

namespace ZooWorld.Game.Views
{
    /// <summary>How a walking animal moves its legs. Steps follow distance travelled, not time.</summary>
    [CreateAssetMenu(menuName = "Zoo World/Views/Gait Preset", fileName = "GaitPreset")]
    public sealed class GaitPreset : ScriptableObject
    {
        [Tooltip("Full leg cycles per meter travelled. Higher means quicker, shorter steps.")]
        [SerializeField, Min(0.01f)] private float _cyclesPerMeter = 1.5f;

        [Tooltip("Speed, in m/s, at which legs reach their full swing. Slower animals swing less.")]
        [SerializeField, Min(0.01f)] private float _fullSwingSpeed = 1f;

        [Header("Leg motion")]
        [Tooltip("Sweep around the hip's vertical axis, in degrees.")]
        [SerializeField, Range(0f, 60f)] private float _swingDegrees = 20f;

        [Tooltip("Forward and back travel of the leg, in meters.")]
        [SerializeField, Min(0f)] private float _stride = 0.1f;

        [Tooltip("How high a leg lifts while it moves forward, in meters.")]
        [SerializeField, Min(0f)] private float _lift = 0.04f;

        [Tooltip("How quickly the legs settle when the animal stops or starts.")]
        [SerializeField, Min(0.1f)] private float _blendSharpness = 8f;

        public float CyclesPerMeter => _cyclesPerMeter;
        public float FullSwingSpeed => _fullSwingSpeed;
        public float SwingDegrees => _swingDegrees;
        public float Stride => _stride;
        public float Lift => _lift;
        public float BlendSharpness => _blendSharpness;
    }
}
