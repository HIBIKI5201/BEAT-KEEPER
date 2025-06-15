using DG.Tweening;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     敵の攻撃警告UI
    /// </summary>
    public class EnemyIndicator : UIElement_RingIndicator
    {
        [Header("色設定")]
        [SerializeField] private Color _warningColor = Color.red;
        [SerializeField] private Color _successColor = Color.yellow;
        [SerializeField] private Color _defaultColor = Color.white;

        [SerializeField] private float _blinkDuration = 0.2f;
        [SerializeField] private float _fadeDuration = 0.3f;

        public override int EffectLength => 5;

        /// <summary>
        /// ビートごとに実行される処理
        /// </summary>
        /// <param name="count"></param>
        public override void Effect(int count)
        {
            base.Effect(count);

            switch (count)
            {
                case 1:
                    _tweens = new Tween[3];
                    Effect1();
                    break;

                case 3:
                    Effect2();
                    break;

                case 5:
                    Effect3();
                    break;
            }
        }

        /// <summary>
        /// 点滅シークエンス
        /// </summary>
        private void Effect1()
        {
            _ringImage.rectTransform.localScale = Vector3.one * _initialScale;

            var blinkSequence = DOTween.Sequence().SetAutoKill(false);

            blinkSequence.Append(_ringImage.DOColor(_warningColor, _blinkDuration).SetLoops(3, LoopType.Yoyo));
            blinkSequence.Join(_selfImage.DOColor(_warningColor, _blinkDuration).SetLoops(3, LoopType.Yoyo));

            blinkSequence.Append(_ringImage.DOColor(_defaultColor, 0.2f).SetEase(Ease.OutQuint));
            blinkSequence.Join(_selfImage.DOColor(_defaultColor, 0.2f).SetEase(Ease.OutQuint));

            blinkSequence.Play();

            _tweens[0] = blinkSequence;
        }

        /// <summary>
        /// リングの縮小
        /// </summary>
        private void Effect2()
        {
            _tweens[1] = _ringImage.rectTransform
                .DOScale(Vector3.one, (float)MusicEngineHelper.DurationOfBeat * 2 - 0.15f)
                .SetEase(Ease.Linear);
        }

        /// <summary>
        /// 当たりエフェクト
        /// </summary>
        private void Effect3()
        {
            var successSequence = DOTween.Sequence();

            successSequence.Append(_selfImage.rectTransform.DOPunchScale(Vector3.one * 0.3f, _blinkDuration, 2, 0.5f));
            successSequence.Join(_selfImage.DOColor(_successColor, _fadeDuration).SetEase(Ease.OutFlash));
            successSequence.Join(_ringImage.DOColor(_successColor, _fadeDuration).SetEase(Ease.OutFlash));
            successSequence.Join(_selfImage.DOFade(0f, _fadeDuration).SetEase(Ease.OutQuint));
            successSequence.Join(_ringImage.DOFade(0f, _fadeDuration).SetEase(Ease.OutQuint));

            successSequence.Play();

            successSequence.OnComplete(End);

            _tweens[2] = successSequence;
        }
    }
}
