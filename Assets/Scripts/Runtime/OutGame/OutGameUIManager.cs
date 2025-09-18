using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Outgame.UI
{
    public class OutGameUIManager : MonoBehaviour
    {
        public void MoveSelection(bool isLanguageSetting, int direction)
        {
            if (isLanguageSetting)
            {
                _languageButton.MoveSelection(direction);
            }
            else
            {
                _subtitleButton.MoveSelection(direction);
            }
        }

        /// <summary>
        /// タイトル画面の表示に戻す
        /// </summary>
        public void HideSettingCanvas()
        {
            IsActiveCanvas(false, _mainCanvasGroup, 0.3f);
            IsActiveCanvas(true, _baseCanvasGroup, 0.3f);
        }
        
        /// <summary>
        /// 設定Canvasを表示する
        /// </summary>
        public void ShowSettingCanvas()
        {
            IsActiveCanvas(true, _mainCanvasGroup, 0.3f);
            IsActiveCanvas(false, _baseCanvasGroup, 0.3f);
            
            // 言語設定キャンバスを表示。字幕は非表示
            IsActiveCanvas(true, _languageCanvasGroup, 0.3f);
            IsActiveCanvas(false, _subtitleCanvasGroup, 0.3f);
            
            _languageButton.Setup();
        }
        
        /// <summary>
        /// 字幕設定キャンバスを表示。言語設定キャンバスを非表示
        /// </summary>
        public void ShowSubtitleCanvas()
        {
            IsActiveCanvas(true, _subtitleCanvasGroup, 0.3f);
            IsActiveCanvas(false, _languageCanvasGroup, 0.3f);

            _subtitleButton.Setup();
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
        
        [SerializeField] private Image _curtainImage;
        [SerializeField] private Image _pressAnyButtonImage;
        [SerializeField, Tooltip("どのようにPressAnyButtonがフェードするかを設定")] private AnimationCurve _fadeCurve;
        [SerializeField, Tooltip("PressAnyButtonのフェードが一周する時間を設定")] private float _fadeDuration = 1f;
        [SerializeField, Tooltip("ボタンを押した際にPressAnyButtonがどのような色になるのか")] private Color _onStartColor;
        [SerializeField, Tooltip("シーン起動時にフェード院にかける時間")] private float _fadeInDuration = 1f;
        [SerializeField, Tooltip("ゲーム開始時にフェードアウトにかかる時間")] private float _fadeOutDuration = 3f;
        
        [Header("初期状態で表示されているロゴ・ボタンのキャンバス")]
        [SerializeField] private CanvasGroup _baseCanvasGroup;
        
        [Header("言語・字幕設定")]
        [SerializeField] private CanvasGroup _mainCanvasGroup;
        [SerializeField] private CanvasGroup _languageCanvasGroup;
        [SerializeField] private CanvasGroup _subtitleCanvasGroup;
        
        [SerializeField] private SettingUIManager _languageButton;
        [SerializeField] private SettingUIManager _subtitleButton;

        private bool _isGameStarted = false;

        public int LanguageId => _languageButton.CurrentButtonIndex;
        
        public int SubtitleId => _subtitleButton.CurrentButtonIndex;

        private void Start()
        {
            _curtainImage.color = new Color(0f, 0f, 0f, 1f);
            _curtainImage.DOFade(0f, _fadeInDuration);

            // ベースキャンバスのみ表示。他は非表示にしておく
            IsActiveCanvas(true, _baseCanvasGroup, 0.3f);
            IsActiveCanvas(false, _mainCanvasGroup, 0.3f);
        }

        private void FixedUpdate()
        {
            if (!_isGameStarted)
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
        /// キャンバスグループの表示・非表示をDOFadeで切り替える
        /// </summary>
        private void IsActiveCanvas(bool isActive, CanvasGroup canvasGroup, float duration)
        {
            canvasGroup.DOFade(isActive ? 1f : 0f, duration);
            canvasGroup.interactable = isActive;
            canvasGroup.blocksRaycasts = isActive;
        }
    }
}
