using R3;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// ボーナススコア倍率を表示するテキスト
    /// </summary>
    [RequireComponent(typeof(Text), typeof(CanvasGroup))]
    public class UIElement_BonusMultiplyText : MonoBehaviour
    {
        [SerializeField] private ScoreManager _scoreManager;
        private Text _text;
        private CanvasGroup _canvasGroup;
        private CompositeDisposable _disposable = new CompositeDisposable();
        
        private void Start()
        {
            _text = GetComponent<Text>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _scoreManager.BonusMultiply.Subscribe(UpdateText).AddTo(_disposable);
        }

        /// <summary>
        /// テキストを更新する
        /// </summary>
        private void UpdateText(float value)
        {
            _text.text = $"BONUS!!! x{value}";

            if (value <= 1)
            {
                Hide(); // 等倍の時はテキストを表示しない
            }
            else if (value > 1 && _canvasGroup.alpha == 0)
            {
                Show();
            }
        }

        private void Show()
        {
            _canvasGroup.alpha = 1;
        }

        private void Hide()
        {
            _canvasGroup.alpha = 0;
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }
    }
}
