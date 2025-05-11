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

        [Header("バトル中")]
        [SerializeField] private CanvasController[] _canvasControllers;
        [SerializeField] private UIElement_ScoreText _scoreText;
        [SerializeField] private UIElement_SpecialMoveIcon _specialMoveIcon;
        
        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            _encounterText.Initialize();
            _scoreText.Initialize();
            _specialMoveIcon.Initialize();
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

        #region バトル中のUI

        /// <summary>
        /// バトル開始時に関連するUIの表示処理を行う
        /// </summary>
        public void BattleStart()
        {
            foreach (CanvasController canvasController in _canvasControllers)
            {
                canvasController.Show();
            }
            _scoreText.SavePreBattleScore(); // バトル前の時点のスコアを保存する
        }

        /// <summary>
        /// バトル終了時に関連するUIを非表示にする処理を行う
        /// </summary>
        public void BattleEnd()
        {
            foreach (CanvasController canvasController in _canvasControllers)
            {
                canvasController.Hide();
            }
        }

        /// <summary>
        /// 必殺技ゲージを更新する
        /// </summary>
        public void SpecialMove(float value) => _specialMoveIcon.FillUpdate(value);

        #endregion
    }
}
