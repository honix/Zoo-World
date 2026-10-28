using UnityEngine;

namespace ZooWorld.Game.Views
{
    /// <summary>Spawn, eat and death animation parameters. Animals can share one preset or have their own.</summary>
    [CreateAssetMenu(menuName = "Zoo World/Views/Animal View Preset", fileName = "AnimalViewPreset")]
    public sealed class AnimalViewPreset : ScriptableObject
    {
        [Header("Spawn: pop in")]
        [SerializeField, Min(0.01f)] private float _spawnDuration = 0.35f;

        [Tooltip("Scale over time, 0 to 1. Go above 1 for an overshoot.")]
        [SerializeField] private AnimationCurve _spawnScale = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 3f), new Keyframe(0.6f, 1.15f), new Keyframe(1f, 1f));

        [Header("Eat: punch")]
        [SerializeField] private Vector3 _eatPunch = new Vector3(0.3f, 0.3f, 0.3f);
        [SerializeField, Min(0.01f)] private float _eatDuration = 0.3f;

        [Header("Death: poof")]
        [SerializeField, Min(0.01f)] private float _deathDuration = 0.3f;

        [Tooltip("Shrink progress over time, 0 (full size) to 1 (gone). Dip below 0 to swell first.")]
        [SerializeField] private AnimationCurve _deathShrink = new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.3f, -0.25f), new Keyframe(1f, 1f));

        [SerializeField] private float _deathSpinDegrees = 200f;

        public float SpawnDuration => _spawnDuration;
        public AnimationCurve SpawnScale => _spawnScale;
        public Vector3 EatPunch => _eatPunch;
        public float EatDuration => _eatDuration;
        public float DeathDuration => _deathDuration;
        public AnimationCurve DeathShrink => _deathShrink;
        public float DeathSpinDegrees => _deathSpinDegrees;
    }
}
