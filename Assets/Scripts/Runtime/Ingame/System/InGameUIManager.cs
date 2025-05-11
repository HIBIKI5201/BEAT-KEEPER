using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper
{
    /// <summary>
    /// インゲームのUIを管理するマネージャークラス
    /// </summary>
    public class InGameUIManager : MonoBehaviour
    {
        [Header("開始演出")]
        [SerializeField] private CanvasGroup _encounterTextCanvasGroup;
        private Text _encountText;
        private float _defaultTextPosY;

        private void Start()
        {
            _encountText = _encounterTextCanvasGroup.GetComponent<Text>();
            _defaultTextPosY = _encountText.transform.position.y;
        }
        
        /// <summary>
        /// 遭遇時のテキストを表示する
        /// </summary>
        public void ShowEncounterText(int battleNumber)
        {
            _encountText.text = $"Mission {battleNumber}";
            _encountText.transform.DOLocalMoveY(300f, 0.5f); // 遭遇UIを上からスライド
            
            _encounterTextCanvasGroup.DOFade(1, 0.5f);
        }

        /// <summary>
        /// 遭遇時のテキストを非表示にする
        /// </summary>
        public void HideEncounterText()
        {
            _encounterTextCanvasGroup.DOFade(0, 0.5f);
            _encountText.transform.DOLocalMoveY(_defaultTextPosY, 0.5f);
        }
    }
}
