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

        private void Awake()
        {
            _animal = GetComponentInParent<Animal>();
            _restPosition = transform.localPosition;
            _restScale = transform.localScale;

            _animal.Spawned += OnSpawned;
            _animal.Died += Unsubscribe;
        }

        private void OnDestroy()
        {
            Unsubscribe();
            if (_animal == null) return;

            _animal.Spawned -= OnSpawned;
            _animal.Died -= Unsubscribe;
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

        private void OnJumped()
        {
            Rest();
            var duration = _cues.JumpDuration;
            Tween.LocalPositionY(transform, _restPosition.y, _restPosition.y + _preset.Height, duration, _preset.Arc);
            Tween.Scale(transform, Vector3.Scale(_restScale, _preset.TakeOffStretch), _preset.TakeOffDuration, Ease.OutQuad)
                .Chain(Tween.Scale(transform, _restScale, Mathf.Max(0.01f, duration - _preset.TakeOffDuration), Ease.InOutSine));
        }

        private void OnLanded()
        {
            Rest();
            Tween.Scale(transform, Vector3.Scale(_restScale, _preset.LandingSquash), _restScale, _preset.LandingDuration, Ease.OutBack);
        }

        private void Rest()
        {
            Tween.StopAll(transform);
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
