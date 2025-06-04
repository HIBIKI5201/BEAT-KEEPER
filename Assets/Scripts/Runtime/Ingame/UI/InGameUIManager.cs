using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.UI
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
        [SerializeField] private UIElement_SeekBar _seekBar;
        [SerializeField] private UIElement_FinisherGuide _finisherGuide;
        [SerializeField] private UIElement_AttackWarningIndicator _warningIndicator;
        [SerializeField] private UIElement_HealthBar _healthBar;

        public UIElement_HealthBar HealthBar => _healthBar;
        private void Start()
        {
            ValidateComponents();
            Initialize();
        }

        /// <summary>
        /// コンポーネントの検証を行う
        /// </summary>
        private void ValidateComponents()
        {
            Debug.Assert(_encounterText != null, "encounterText が設定されていません");
            Debug.Assert(_canvasControllers != null && _canvasControllers.Length > 0, "canvasControllers が設定されていません");
            Debug.Assert(_scoreText != null, "scoreText が設定されていません");
            Debug.Assert(_seekBar != null, "seekBar が設定されていません");
            Debug.Assert(_finisherGuide != null, "finisherGuide が設定されていません");
            Debug.Assert(_warningIndicator != null, "warningIndicatorが設定されていません");
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

            PrepareUIElements();
        }

        /// <summary>
        /// UI要素の準備
        /// </summary>
        private void PrepareUIElements()
        {
            _scoreText.SavePreBattleScore(); // バトル前の時点のスコアを保存する
            _seekBar.Initialize();
            _finisherGuide.CountReset();
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
        /// フィニッシャーガイドを表示
        /// </summary>
        public void ShowFinisherGuide() => _finisherGuide.Show();

        /// <summary>
        /// フィニッシャーガイドを非表示
        /// </summary>
        public void HideFinisherGuide() => _finisherGuide.Hide();

        #endregion
    }
}
