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
        private Text _text;
        private CanvasGroup _canvasGroup;
        private int _count;
        
        public void Show()
        {
            _count++;
            if (_count <= 1)
            {
                _text.text = _activation; // 1回目はフィニッシャー突入時の文字列を表示する
            }
            else
            {
                _text.text = _consecutiveHits; // 2回目以降は連打時の文字列を表示する
            }
            _canvasGroup.DOFade(1, 0.5f);
        }
        
        public void Hide()
        {
            _canvasGroup.DOFade(0f, 1f);
        }
        
        public void CountReset() => _count = 0;
    }
}
