using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// 開始演出中に表示する遭遇テキストの処理を行うコンポーネント
    /// </summary>
    [RequireComponent(typeof(CanvasGroup), typeof(Text))]
    public class UIElement_EncounterText : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private Text _text;
        private float _defaultTextPosY;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _text = GetComponent<Text>();
            _defaultTextPosY = transform.localPosition.y; // 初期のY座標を保存しておく
        }

        private void Start()
        {
            _canvasGroup.alpha = 0;
        }

        /// <summary>
        /// 遭遇時のテキストを表示する
        /// </summary>
        public void ShowEncounterText(int battleNumber)
        {
            _text.text = $"Mission {battleNumber}";
            transform.DOLocalMoveY(300f, 0.5f); // 遭遇UIを上からスライド
            _canvasGroup.DOFade(1, 1f);
        }

        /// <summary>
        /// 遭遇時のテキストを非表示にする
        /// </summary>
        public void HideEncounterText()
        {
            _canvasGroup.DOFade(0, 0.5f);
            _text.transform.DOLocalMoveY(_defaultTextPosY, 0.5f);
        }
    }
}
