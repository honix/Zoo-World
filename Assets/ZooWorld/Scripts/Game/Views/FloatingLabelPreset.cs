using UnityEngine;

namespace ZooWorld.Game.Views
{
    /// <summary>Pop, float and fade of a short-lived world label such as "Tasty!".</summary>
    [CreateAssetMenu(menuName = "Zoo World/Views/Floating Label Preset", fileName = "FloatingLabelPreset")]
    public sealed class FloatingLabelPreset : ScriptableObject
    {
        [SerializeField, Min(0.1f)] private float _lifetime = 1.2f;

        [Header("Pop")]
        [SerializeField, Min(0.01f)] private float _popDuration = 0.3f;
        [SerializeField] private AnimationCurve _popScale = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 5f), new Keyframe(0.55f, 1.3f), new Keyframe(1f, 1f));

        [Header("Float")]
        [Tooltip("How far the label drifts up the screen over its lifetime, in meters.")]
        [SerializeField] private float _rise = 0.8f;
        [SerializeField] private AnimationCurve _riseOverLifetime = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 2f), new Keyframe(1f, 1f));

        [Header("Fade")]
        [SerializeField] private AnimationCurve _alphaOverLifetime = new AnimationCurve(
            new Keyframe(0f, 1f), new Keyframe(0.65f, 1f), new Keyframe(1f, 0f));

        public float Lifetime => _lifetime;
        public float PopDuration => _popDuration;
        public AnimationCurve PopScale => _popScale;
        public float Rise => _rise;
        public AnimationCurve RiseOverLifetime => _riseOverLifetime;
        public AnimationCurve AlphaOverLifetime => _alphaOverLifetime;
    }
}
