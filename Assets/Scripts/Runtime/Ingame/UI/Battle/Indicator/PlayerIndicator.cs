using BeatKeeper.Runtime.Ingame.UI;
using BeatKeeper.Runtime.System;
using DG.Tweening;
using UnityEngine;

namespace BeatKeeper
{
    public class PlayerIndicator : UIElement_RingIndicator
    {
        [Header("色設定")]
        [SerializeField] private Color _successColor = Color.yellow;
        [SerializeField] private Color _defaultColor = Color.white;

        [SerializeField] private float _blinkDuration = 0.2f;
        [SerializeField] private float _fadeDuration = 0.3f;

        public override int EffectLength => 3;

        public override void Effect(int count)
        {
            switch(count)
            {
                case 1:
                    Effect1();
                    break;
                case 3:
                    Effect2();
                    break;
            }
        }

        /// <summary>
        /// リングの縮小
        /// </summary>
        private void Effect1()
        {
            _ringImage.rectTransform.localScale = Vector3.one * _initialScale;
            
            _ringImage.color = _defaultColor;
            _selfImage.color = _defaultColor;

            _ringImage.rectTransform.DOScale(Vector3.one, (float)MusicEngineHelper.DurationOfBeat * 2 - 0.15f).SetEase(Ease.Linear);
        }

        /// <summary>
        /// 当たりエフェクト
        /// </summary>
        private void Effect2()
        {
            var successSequence = DOTween.Sequence();

            successSequence.Append(_selfImage.rectTransform.DOPunchScale(Vector3.one * 0.3f, _blinkDuration, 2, 0.5f));
            successSequence.Join(_selfImage.DOColor(_successColor, _fadeDuration).SetEase(Ease.OutFlash));
            successSequence.Join(_ringImage.DOColor(_successColor, _fadeDuration).SetEase(Ease.OutFlash));
            successSequence.Join(_selfImage.DOFade(0f, _fadeDuration).SetEase(Ease.OutQuint));
            successSequence.Join(_ringImage.DOFade(0f, _fadeDuration).SetEase(Ease.OutQuint));

            successSequence.Play();

            successSequence.OnComplete(_onEndAction.Invoke);
        }
    }
}
