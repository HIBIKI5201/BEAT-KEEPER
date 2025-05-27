using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    public class UIElement_HealthBar : MonoBehaviour
    {
        [SerializeField] private Image _bar;

        private void Start()
        {
            if (!_bar) Debug.LogWarning("bar is null");
        }

        public void SetBarAmount(float amount) => _bar.fillAmount = amount;
    }
}