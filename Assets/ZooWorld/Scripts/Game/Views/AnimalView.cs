using System;
using PrimeTween;
using UnityEngine;
using ZooWorld.Game.Animals;

namespace ZooWorld.Game.Views
{
    /// <summary>
    /// Generic look-and-feel for any animal, on its Visual object: pops in on spawn,
    /// punches when it eats, poofs when it dies. It only listens to the animal's events;
    /// logic never waits for it, except the pool for the death animation.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class AnimalView : MonoBehaviour, IDeathAnimation
    {
        [SerializeField] private AnimalViewPreset _preset;

        private Animal _animal;
        private Vector3 _restScale;
        private Quaternion _restRotation;

        private void Awake()
        {
            _animal = GetComponentInParent<Animal>();
            _restScale = transform.localScale;
            _restRotation = transform.localRotation;

            _animal.Spawned += OnSpawned;
            _animal.Ate += OnAte;
        }

        private void OnDestroy()
        {
            if (_animal == null) return;

            _animal.Spawned -= OnSpawned;
            _animal.Ate -= OnAte;
        }

        public void Play(Action onFinished)
        {
            Reset();
            var spin = _restRotation * Quaternion.Euler(0f, _preset.DeathSpinDegrees, 0f);
            Sequence.Create()
                .Group(Tween.Scale(transform, _restScale, Vector3.zero, _preset.DeathDuration, _preset.DeathShrink))
                .Group(Tween.LocalRotation(transform, spin, _preset.DeathDuration, Ease.InQuad))
                .OnComplete(() =>
                {
                    Reset();
                    onFinished();
                }, warnIfTargetDestroyed: false);
        }

        private void OnSpawned()
        {
            Reset();
            Tween.Scale(transform, Vector3.zero, _restScale, _preset.SpawnDuration, _preset.SpawnScale);
        }

        private void OnAte()
        {
            Tween.StopAll(transform);
            transform.localScale = _restScale;
            Tween.PunchScale(transform, _preset.EatPunch, _preset.EatDuration);
        }

        private void Reset()
        {
            Tween.StopAll(transform);
            transform.localScale = _restScale;
            transform.localRotation = _restRotation;
        }
    }
}
