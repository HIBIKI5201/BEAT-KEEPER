using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// 開始演出中に表示するスキップの処理を行うコンポーネント
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class UIElement_StartSkipGuage : BaseSkipController
    {
        [SerializeField, Tooltip("スキップまでの割合")]
        private Image _guage;

        private CanvasGroup _canvasGroup;

        private Tween _fadeTween;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected override void Start()
        {
            _canvasGroup.alpha = 0;
        }

        /// <summary>
        /// 遭遇時のテキストを表示する
        /// </summary>
        public void ShowEncounterText(int battleNumber)
        {
            _fadeTween = _canvasGroup.DOFade(1, 1f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            _onHoldTimeChanged += HandleHoldTimeChanged;
        }

        /// <summary>
        /// 遭遇時のテキストを非表示にする
        /// </summary>
        public void HideEncounterText()
        {
            _onHoldTimeChanged -= HandleHoldTimeChanged;
            _fadeTween?.Kill();
            _canvasGroup.DOFade(0, 0.5f);
        }

        protected override void OnSkip() => HideEncounterText();

        private void HandleHoldTimeChanged(float p)
        {
            _guage.fillAmount = p;
        }
    }
}
