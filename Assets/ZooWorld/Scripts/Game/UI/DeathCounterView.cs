using PrimeTween;
using TMPro;
using UnityEngine;

namespace ZooWorld.Game.UI
{
    /// <summary>Passive view of the death counter: it shows numbers and knows nothing about the zoo.</summary>
    public sealed class DeathCounterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _deadPrey;
        [SerializeField] private TMP_Text _deadPredators;

        [Tooltip("Scale punch on a number when it changes.")]
        [SerializeField] private Vector3 _changePunch = new Vector3(0.35f, 0.35f, 0f);
        [SerializeField, Min(0.01f)] private float _changeDuration = 0.3f;

        private int _shownPrey = -1;
        private int _shownPredators = -1;

        public void Show(int deadPrey, int deadPredators)
        {
            Set(_deadPrey, ref _shownPrey, deadPrey);
            Set(_deadPredators, ref _shownPredators, deadPredators);
        }

        private void Set(TMP_Text text, ref int shown, int value)
        {
            if (shown == value) return;

            var isFirst = shown < 0;
            shown = value;
            text.SetText("{0}", value);
            if (isFirst || !Application.isPlaying) return;

            Tween.StopAll(text.transform);
            text.transform.localScale = Vector3.one;
            Tween.PunchScale(text.transform, _changePunch, _changeDuration);
        }
    }
}
