using UnityEngine;
using ZooWorld.Game.Animals;

namespace ZooWorld.Game.Views
{
    /// <summary>
    /// Eases the look towards the animal's heading instead of snapping to it,
    /// and leans it into turns. Logic and physics keep their own rotation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TurnView : MonoBehaviour
    {
        [SerializeField] private TurnPreset _preset;

        private Animal _animal;
        private Quaternion _facing;
        private float _yaw;
        private float _lean;

        private void Awake()
        {
            _animal = GetComponentInParent<Animal>();
            _animal.Spawned += Snap;
        }

        private void OnDestroy()
        {
            if (_animal != null) _animal.Spawned -= Snap;
        }

        private void Snap()
        {
            _facing = Quaternion.LookRotation(_animal.Movement.Heading);
            _yaw = _facing.eulerAngles.y;
            _lean = 0f;
            transform.rotation = _facing;
        }

        private void LateUpdate()
        {
            // Once dead, the death animation owns the rotation.
            if (!_animal.IsAlive) return;

            var deltaTime = Mathf.Max(Time.deltaTime, 1e-4f);
            var target = Quaternion.LookRotation(_animal.Movement.Heading);
            _facing = Quaternion.Slerp(_facing, target, Damping(_preset.TurnSharpness, deltaTime));

            var yaw = _facing.eulerAngles.y;
            var turnSpeed = Mathf.DeltaAngle(_yaw, yaw) / deltaTime;
            _yaw = yaw;

            // Turning right leans right, like a bike.
            var targetLean = Mathf.Clamp(-turnSpeed * _preset.LeanPerTurnSpeed, -_preset.MaxLean, _preset.MaxLean);
            _lean = Mathf.Lerp(_lean, targetLean, Damping(_preset.LeanSharpness, deltaTime));

            transform.rotation = _facing * Quaternion.Euler(0f, 0f, _lean);
        }

        /// <summary>Frame-rate independent exponential smoothing factor.</summary>
        private static float Damping(float sharpness, float deltaTime) => 1f - Mathf.Exp(-sharpness * deltaTime);
    }
}
