using UnityEngine;

namespace ZooWorld.Game.Views
{
    /// <summary>How smoothly an animal's look turns towards its heading, and how much it leans into turns.</summary>
    [CreateAssetMenu(menuName = "Zoo World/Views/Turn Preset", fileName = "TurnPreset")]
    public sealed class TurnPreset : ScriptableObject
    {
        [Tooltip("How quickly the look catches up with the heading. Higher is snappier.")]
        [SerializeField, Min(0.1f)] private float _turnSharpness = 9f;

        [Tooltip("Lean (roll) in degrees per degree-per-second of turning.")]
        [SerializeField, Min(0f)] private float _leanPerTurnSpeed = 0.06f;

        [SerializeField, Range(0f, 45f)] private float _maxLean = 18f;

        [Tooltip("How quickly the lean follows the turn. Higher is snappier.")]
        [SerializeField, Min(0.1f)] private float _leanSharpness = 7f;

        public float TurnSharpness => _turnSharpness;
        public float LeanPerTurnSpeed => _leanPerTurnSpeed;
        public float MaxLean => _maxLean;
        public float LeanSharpness => _leanSharpness;
    }
}
