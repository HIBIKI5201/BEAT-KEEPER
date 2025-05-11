using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// インゲームのUIを管理するマネージャークラス
    /// </summary>
    public class InGameUIManager : MonoBehaviour
    {
        [Header("開始演出")]
        [SerializeField] private UIElement_EncounterText _encounterText;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            _encounterText.Initialize();
        }

        #region 開始演出関連のUI
        
        /// <summary>
        /// 遭遇時のテキストを表示する
        /// </summary>
        public void ShowEncounterText(int battleNumber) => _encounterText.ShowEncounterText(battleNumber);

        /// <summary>
        /// 遭遇時のテキストを非表示にする
        /// </summary>
        public void HideEncounterText() => _encounterText.HideEncounterText();
        
        #endregion
    }
}
