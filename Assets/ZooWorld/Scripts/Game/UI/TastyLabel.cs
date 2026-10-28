using System;
using TMPro;
using UnityEngine;
using ZooWorld.Core.Animals;
using ZooWorld.Game.Animals;
using ZooWorld.Game.Views;

namespace ZooWorld.Game.UI
{
    /// <summary>
    /// The "Tasty!" text shown under a predator that has just eaten: it pops, floats and fades.
    /// It follows its predator while that one is alive, then finishes in place.
    /// </summary>
    public sealed class TastyLabel : MonoBehaviour
    {
        [Tooltip("Offset from the predator. Negative Z is 'below' on a top-down screen.")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 1.2f, -1f);

        [SerializeField] private FloatingLabelPreset _preset;
        [SerializeField] private TMP_Text _text;

        private Animal _target;
        private AnimalId _targetId;
        private Vector3 _anchor;
        private float _age;
        private Color _color;
        private Action<TastyLabel> _onFinished;

        private void Awake() => _color = _text.color;

        public void Show(Animal target, Action<TastyLabel> onFinished)
        {
            _target = target;
            _targetId = target.Id;
            _onFinished = onFinished;
            _age = 0f;
            Animate();
        }

        private void LateUpdate()
        {
            _age += Time.deltaTime;
            Animate();

            if (_age >= _preset.Lifetime) _onFinished?.Invoke(this);
        }

        private void Animate()
        {
            // Pooled animals get reused: only follow the same, still living animal.
            if (_target != null && _target.IsAlive && _target.Id == _targetId) _anchor = _target.transform.position;

            var life = Mathf.Clamp01(_age / _preset.Lifetime);
            var rise = _preset.RiseOverLifetime.Evaluate(life) * _preset.Rise;
            transform.position = _anchor + _offset + Vector3.forward * rise;
            transform.localScale = Vector3.one * _preset.PopScale.Evaluate(Mathf.Clamp01(_age / _preset.PopDuration));
            _text.color = new Color(_color.r, _color.g, _color.b, _color.a * _preset.AlphaOverLifetime.Evaluate(life));
        }
    }
}
