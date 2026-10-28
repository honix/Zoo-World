using System;
using UnityEngine;
using ZooWorld.Game.Animals;

namespace ZooWorld.Game.Views
{
    /// <summary>
    /// Walks an animal's legs while it moves: each leg sweeps, strides and lifts with its own
    /// phase in the step cycle. The cycle advances with distance travelled, so feet don't
    /// skate, and the legs settle back to rest when the animal stops.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LegGaitView : MonoBehaviour
    {
        [Serializable]
        public struct Leg
        {
            [Tooltip("The leg's hip: it rotates and moves from here.")]
            public Transform Hip;

            [Tooltip("Offset in the step cycle, 0..1. Legs half a cycle apart alternate.")]
            [Range(0f, 1f)] public float Phase;
        }

        [SerializeField] private GaitPreset _preset;
        [SerializeField] private Leg[] _legs = new Leg[0];

        private Animal _animal;
        private Vector3[] _restPositions;
        private Quaternion[] _restRotations;
        private float _cycle;
        private float _amount;

        private void Awake()
        {
            _animal = GetComponentInParent<Animal>();
            _restPositions = new Vector3[_legs.Length];
            _restRotations = new Quaternion[_legs.Length];
            for (var i = 0; i < _legs.Length; i++)
            {
                _restPositions[i] = _legs[i].Hip.localPosition;
                _restRotations[i] = _legs[i].Hip.localRotation;
            }

            _animal.Spawned += OnSpawned;
        }

        private void OnDestroy()
        {
            if (_animal != null) _animal.Spawned -= OnSpawned;
        }

        private void OnSpawned()
        {
            _cycle = 0f;
            _amount = 0f;
            Pose();
        }

        private void LateUpdate()
        {
            var speed = _animal.IsAlive ? _animal.Velocity.magnitude : 0f;
            var deltaTime = Time.deltaTime;

            _cycle += speed * _preset.CyclesPerMeter * deltaTime;
            var target = Mathf.Clamp01(speed / _preset.FullSwingSpeed);
            _amount = Mathf.Lerp(_amount, target, 1f - Mathf.Exp(-_preset.BlendSharpness * deltaTime));

            Pose();
        }

        private void Pose()
        {
            for (var i = 0; i < _legs.Length; i++)
            {
                Gait.Evaluate(_cycle, _legs[i].Phase, out var swing, out var lift);
                swing *= _amount;
                lift *= _amount;

                var hip = _legs[i].Hip;
                hip.localRotation = Quaternion.Euler(0f, swing * _preset.SwingDegrees, 0f) * _restRotations[i];
                hip.localPosition = _restPositions[i] + new Vector3(0f, lift * _preset.Lift, swing * _preset.Stride);
            }
        }
    }
}
