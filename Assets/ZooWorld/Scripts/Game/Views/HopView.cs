using PrimeTween;
using UnityEngine;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Movement;

namespace ZooWorld.Game.Views
{
    /// <summary>
    /// Makes a jumping animal hop: stretch on take-off, an arc in the air, squash on landing.
    /// Works with any movement that offers <see cref="IJumpCues"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class HopView : MonoBehaviour
    {
        [SerializeField] private HopPreset _preset;

        private Animal _animal;
        private IJumpCues _cues;
        private Vector3 _restPosition;
        private Vector3 _restScale;
        private Sequence _animation;

        private void Awake()
        {
            _animal = GetComponentInParent<Animal>();
            _restPosition = transform.localPosition;
            _restScale = transform.localScale;

            _animal.Spawned += OnSpawned;
            _animal.Died += OnDied;
        }

        private void OnDestroy()
        {
            if (_animation.isAlive) _animation.Stop();
            Unsubscribe();
            if (_animal == null) return;

            _animal.Spawned -= OnSpawned;
            _animal.Died -= OnDied;
        }

        private void OnSpawned()
        {
            Unsubscribe();
            Rest();

            _cues = _animal.Movement as IJumpCues;
            if (_cues == null) return;

            _cues.Jumped += OnJumped;
            _cues.Landed += OnLanded;
        }

        private void OnDied()
        {
            if (_animation.isAlive) _animation.Stop();
            Unsubscribe();
        }

        private void OnJumped()
        {
            Rest();
            var duration = _cues.JumpDuration;
            var stretch = Sequence.Create(Tween.Scale(transform, Vector3.Scale(_restScale, _preset.TakeOffStretch), _preset.TakeOffDuration, Ease.OutQuad))
                .Chain(Tween.Scale(transform, _restScale, Mathf.Max(0.01f, duration - _preset.TakeOffDuration), Ease.InOutSine));
            _animation = Sequence.Create(Tween.LocalPositionY(transform, _restPosition.y, _restPosition.y + _preset.Height, duration, _preset.Arc))
                .Group(stretch);
        }

        private void OnLanded()
        {
            Rest();
            _animation = Sequence.Create(Tween.Scale(transform, Vector3.Scale(_restScale, _preset.LandingSquash), _restScale, _preset.LandingDuration, Ease.OutBack));
        }

        private void Rest()
        {
            if (_animation.isAlive) _animation.Stop();
            transform.localPosition = _restPosition;
            transform.localScale = _restScale;
        }

        private void Unsubscribe()
        {
            if (_cues == null) return;

            _cues.Jumped -= OnJumped;
            _cues.Landed -= OnLanded;
            _cues = null;
        }
    }
}
