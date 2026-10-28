using System;
using UnityEngine;
using ZooWorld.Core.Animals;
using ZooWorld.Game.Animals;

namespace ZooWorld.Game.UI
{
    /// <summary>
    /// The "Tasty!" text shown under a predator that has just eaten.
    /// It follows its predator while that one is alive, then finishes in place.
    /// </summary>
    public sealed class TastyLabel : MonoBehaviour
    {
        [Tooltip("Offset from the predator. Negative Z is 'below' on a top-down screen.")]
        [SerializeField] private Vector3 _offset = new Vector3(0f, 1.2f, -1f);

        [SerializeField, Min(0.1f)] private float _lifetime = 1.2f;

        private Animal _target;
        private AnimalId _targetId;
        private float _age;
        private Action<TastyLabel> _onFinished;

        public void Show(Animal target, Action<TastyLabel> onFinished)
        {
            _target = target;
            _targetId = target.Id;
            _onFinished = onFinished;
            _age = 0f;
            Follow();
        }

        private void LateUpdate()
        {
            Follow();

            _age += Time.deltaTime;
            if (_age >= _lifetime) _onFinished?.Invoke(this);
        }

        private void Follow()
        {
            // Pooled animals get reused: only follow the same, still living animal.
            if (_target == null || !_target.IsAlive || _target.Id != _targetId) return;

            transform.position = _target.transform.position + _offset;
        }
    }
}
