using TMPro;
using UnityEngine;

namespace ZooWorld.Game.UI
{
    /// <summary>Passive view of the death counter: it shows numbers and knows nothing about the zoo.</summary>
    public sealed class DeathCounterView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _deadPrey;
        [SerializeField] private TMP_Text _deadPredators;

        public void Show(int deadPrey, int deadPredators)
        {
            _deadPrey.SetText("{0}", deadPrey);
            _deadPredators.SetText("{0}", deadPredators);
        }
    }
}
