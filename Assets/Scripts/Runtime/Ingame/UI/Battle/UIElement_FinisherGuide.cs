using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// フィニッシャー時の操作方法を表すテキスト
    /// </summary>
    [RequireComponent(typeof(Text), typeof(CanvasGroup))]
    public class UIElement_FinisherGuide : MonoBehaviour
    {
        [SerializeField] private string _activation = "R2を押せ!"; // フィニッシャー突入時
        [SerializeField] private string _consecutiveHits = "連打!!!"; // 連打時
        [SerializeField] private float _animationDuration = 0.5f;
        [SerializeField] private float _pulseScale = 1.2f;
        [SerializeField] private Color _highlightColor = Color.red;
        [SerializeField] private int _shakeStrength = 5;
        [SerializeField] private int _vibrato = 10;
        
        private Text _text;
        private CanvasGroup _canvasGroup;
        private int _count;
        private Sequence _currentSequence;
        private Vector3 _originalScale;
        private Color _originalColor;
        
        private void Start()
        {
            _text = GetComponent<Text>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _originalScale = transform.localScale;
            _originalColor = _text.color;
            
            // 最初は非表示
            _canvasGroup.alpha = 0f;
        }
        
        public void Show()
        {
            // 既存のアニメーションがあれば停止
            _currentSequence?.Kill();
            
            _count++;
            if (_count <= 1)
            {
                _text.text = _activation; // 1回目はフィニッシャー突入時の文字列を表示する
                ShowActivationAnimation();
            }
            else
            {
                _text.text = _consecutiveHits; // 2回目以降は連打時の文字列を表示する
                ShowConsecutiveHitsAnimation();
            }
        }
        
        private void ShowActivationAnimation()
        {
            transform.localScale = Vector3.zero;
            
            _currentSequence = DOTween.Sequence();
            
            // フェードインしながらポップアップ表示
            _currentSequence.Append(_canvasGroup.DOFade(1, _animationDuration * 0.7f))
                .Join(transform.DOScale(_originalScale * _pulseScale, _animationDuration * 0.7f).SetEase(Ease.OutBack))
                .Append(transform.DOScale(_originalScale, _animationDuration * 0.3f))
                .Append(_text.DOColor(_highlightColor, _animationDuration * 0.5f))
                .Append(_text.DOColor(_originalColor, _animationDuration * 0.5f))
                .SetLoops(3, LoopType.Restart);
        }
        
        private void ShowConsecutiveHitsAnimation()
        {
            _currentSequence = DOTween.Sequence();
            
            _currentSequence.Append(_canvasGroup.DOFade(1, _animationDuration * 0.2f))
                .Join(transform.DOShakePosition(_animationDuration, _shakeStrength, _vibrato))
                .Join(_text.DOColor(_highlightColor, _animationDuration * 0.25f))
                .Append(_text.DOColor(_originalColor, _animationDuration * 0.25f))
                .Join(transform.DOScale(_originalScale * _pulseScale, _animationDuration * 0.25f))
                .Append(transform.DOScale(_originalScale, _animationDuration * 0.25f))
                .SetLoops(-1, LoopType.Restart);
        }
        
        public void Hide()
        {
            // 既存のアニメーションがあれば停止
            _currentSequence?.Kill();
            
            // 新しいフェードアウトアニメーション
            _currentSequence = DOTween.Sequence();
            _currentSequence.Append(_canvasGroup.DOFade(0f, _animationDuration * 0.5f))
                .Join(transform.DOScale(_originalScale * 0.8f, _animationDuration * 0.5f))
                .OnComplete(() => transform.localScale = _originalScale);
        }
        
        public void CountReset()
        {
            _count = 0;
            _currentSequence?.Kill();
            _canvasGroup.alpha = 0f;
            transform.localScale = _originalScale;
            _text.color = _originalColor;
        }
        
        private void OnDestroy()
        {
            _currentSequence?.Kill();
        }
    }
}