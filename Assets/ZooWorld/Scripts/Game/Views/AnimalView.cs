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
        private Sequence _animation;

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
            if (_animation.isAlive) _animation.Stop();
            if (_animal == null) return;

            _animal.Spawned -= OnSpawned;
            _animal.Ate -= OnAte;
        }

        public void Play(Action onFinished)
        {
            Rest();
            var spin = _restRotation * Quaternion.Euler(0f, _preset.DeathSpinDegrees, 0f);
            _animation = Sequence.Create(Tween.Scale(transform, _restScale, Vector3.zero, _preset.DeathDuration, _preset.DeathShrink))
                .Group(Tween.LocalRotation(transform, spin, _preset.DeathDuration, Ease.InQuad))
                .OnComplete(() =>
                {
                    Rest();
                    onFinished();
                }, warnIfTargetDestroyed: false);
        }

        private void OnSpawned()
        {
            Rest();
            _animation = Sequence.Create(Tween.Scale(transform, Vector3.zero, _restScale, _preset.SpawnDuration, _preset.SpawnScale));
        }

        private void OnAte()
        {
            Rest();
            _animation = Sequence.Create(Tween.PunchScale(transform, _preset.EatPunch, _preset.EatDuration));
        }

        private void Rest()
        {
            if (_animation.isAlive) _animation.Stop();
            transform.localScale = _restScale;
            transform.localRotation = _restRotation;
        }
    }
}
