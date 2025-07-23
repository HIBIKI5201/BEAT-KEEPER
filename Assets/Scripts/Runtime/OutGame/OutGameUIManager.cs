using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    public class OutGameUIManager : MonoBehaviour
    {
        [SerializeField] private Image _curtainImage;
        [SerializeField] private Image _pressAnyButtonImage;
        [SerializeField, Tooltip("どのようにPressAnyButtonがフェードするかを設定")] private AnimationCurve _fadeCurve;
        [SerializeField, Tooltip("PressAnyButtonのフェードが一周する時間を設定")] private float _fadeDuration = 1f;
        [SerializeField, Tooltip("ボタンを押した際にPressAnyButtonがどのような色になるのか")] private Color _onStartColor;
        [SerializeField, Tooltip("シーン起動時にフェード院にかける時間")] private float _fadeInDuration = 1f;
        [SerializeField, Tooltip("ゲーム開始時にフェードアウトにかかる時間")] private float _fadeOutDuration = 3f;
        private bool _isGameStarted = false;

        private void Start()
        {
            _curtainImage.color = new Color(0f, 0f, 0f, 1f);
            _curtainImage.DOFade(0f, _fadeInDuration);
        }

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
        public async Task GameStart()
        {
            _isGameStarted = true;
            if (_pressAnyButtonImage != null)
            {
                var color = _pressAnyButtonImage.color;
                color = _onStartColor;
                _pressAnyButtonImage.color = color;
            }
            await _curtainImage.DOFade(1f, _fadeOutDuration).AsyncWaitForCompletion();
        }
    }
}
