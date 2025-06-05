using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using DG.Tweening;
using SymphonyFrameWork.System;
using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    /// 敵の攻撃警告UI
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class UIElement_RingIndicator : MonoBehaviour
    {
        [Header("基本設定")]
        [SerializeField] private Sprite _ringSprite;
        [SerializeField] private float _initialScale = 3.5f;
        [Space]
        [SerializeField] private float _blinkDuration = 0.2f;
        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField, Tooltip("リングの縮小時間の拍数")] private int _reductionTime = 3;

        [Header("色設定")]
        [SerializeField] private Color _warningColor = Color.red;
        [SerializeField] private Color _successColor = Color.yellow;
        [SerializeField] private Color _defaultColor = Color.white;

        private Action _onEndAction;

        private Image _selfImage;
        private Image _ringImage;

        private float _durationOfBeat;

        private void Awake()
        {
            _selfImage = GetComponent<Image>();
            _ringImage = transform.GetChild(0).GetComponent<Image>();
        }

        public void Initialize(float durationOfBeat)
        {
            _durationOfBeat = durationOfBeat;
        }

        public void OnGet(Action onEndAction, Vector2 rectPos)
        {
            _selfImage.rectTransform.position = rectPos;
            _onEndAction = onEndAction;
        }

        public async void EffectStart()
        {
            _ringImage.rectTransform.localScale = Vector3.one * _initialScale;

            // 点滅シーケンス（先に再生開始）
            var blinkSequence = DOTween.Sequence().SetAutoKill(false);

            blinkSequence.Append(_ringImage.DOColor(_warningColor, _blinkDuration).SetLoops(3, LoopType.Yoyo));
            blinkSequence.Join(_selfImage.DOColor(_warningColor, _blinkDuration).SetLoops(3, LoopType.Yoyo));

            blinkSequence.Append(_ringImage.DOColor(_defaultColor, 0.2f).SetEase(Ease.OutQuint));
            blinkSequence.Join(_selfImage.DOColor(_defaultColor, 0.2f).SetEase(Ease.OutQuint));

            blinkSequence.Play();

            // 待機（縮小処理）
            await Awaitable.WaitForSecondsAsync(_durationOfBeat * 2, destroyCancellationToken);

            blinkSequence.Kill();
            _ringImage.rectTransform.DOScale(Vector3.one, _durationOfBeat * _reductionTime - 0.15f).SetEase(Ease.Linear);

            // 指定時間が経過したら成功演出（別シーケンスで独立に動く）
            await Awaitable.WaitForSecondsAsync(_durationOfBeat * _reductionTime, destroyCancellationToken);

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