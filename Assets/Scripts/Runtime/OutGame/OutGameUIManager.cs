using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    public class OutGameUIManager : MonoBehaviour
    {
        [SerializeField] private Image _pressAnyButtonImage;
        [SerializeField] private AnimationCurve _fadeCurve;
        [SerializeField] private float _fadeDuration = 1f;

        private void FixedUpdate()
        {
            var time = Time.time % _fadeDuration;
            var alpha = _fadeCurve.Evaluate(time / _fadeDuration);
            if (_pressAnyButtonImage != null)
            {
                var color = _pressAnyButtonImage.color;
                color.a = alpha;
                _pressAnyButtonImage.color = color;
            }
        }
    }
}
