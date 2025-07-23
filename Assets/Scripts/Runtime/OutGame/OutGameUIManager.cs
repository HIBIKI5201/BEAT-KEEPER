using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    public class OutGameUIManager : MonoBehaviour
    {
        [SerializeField] private Image _pressAnyButtonImage;
        [SerializeField, Tooltip("どのようにPressAnyButtonがフェードするかを設定")] private AnimationCurve _fadeCurve;
        [SerializeField, Tooltip("PressAnyButtonのフェードが一周する時間を設定")] private float _fadeDuration = 1f;
        [SerializeField, Tooltip("ボタンを押した際にPressAnyButtonがどのような色になるのか")] private Color _onStartColor;
        private bool _isGameStarted = false;

        private void FixedUpdate()
        {
            if(!_isGameStarted)
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

        /// <summary>
        /// スタートした際に呼び出されるメソッド。
        /// </summary>
        public void GameStart()
        {
            _isGameStarted = true;
            if (_pressAnyButtonImage != null)
            {
                var color = _pressAnyButtonImage.color;
                color = _onStartColor;
                _pressAnyButtonImage.color = color;
            }
        }
    }
}
