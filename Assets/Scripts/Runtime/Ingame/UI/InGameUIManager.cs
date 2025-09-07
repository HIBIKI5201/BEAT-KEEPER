using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine;
using R3;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    /// インゲームのUIを管理するマネージャークラス
    /// </summary>
    public class InGameUIManager : MonoBehaviour
    {
        /// <summary>
        /// 敵のHPバーを初期化する
        /// </summary>
        /// <param name="enemy"></param>
        public void HealthBarInitialize(EnemyManager enemy)
        {
            _healthBar.RegisterEnemyEvent(enemy);
        }

        /// <summary>
        /// NOTE: チュートリアルのシーケンスから呼び出される
        /// </summary>
        public void BattleStart()
        {
            ShowBattleUI();
        }
        
        [Header("バトル中")]
        [SerializeField] private CanvasController[] _canvasControllers;
        [SerializeField] private UIElement_ScoreText _scoreText;
        [SerializeField] private UIElement_FinisherGuide _finisherGuide;
        [SerializeField] private UIElement_ChartRingManager _chartRingManager;
        [SerializeField] private UIElement_HealthBar _healthBar;

        private PhaseManager _phaseManager;
        private CompositeDisposable _disposables = new CompositeDisposable();
        
        /// <summary>
        /// Start
        /// </summary>
        private async void Start()
        {
            _phaseManager = await ServiceLocator.GetInstanceAsync<PhaseManager>();
            if (_phaseManager != null)
            {
                // ムービー中にはUIが表示されないようにしたいので、フェーズのリアクティブプロパティを購読する
                _phaseManager.CurrentPhaseProp.Subscribe(HandlePhaseChanged).AddTo(_disposables);
            }
            else
            {
                Debug.LogError("フェーズマネージャーが取得できませんでした。ムービー中のUI非表示処理が行われません");
            }
            
            ValidateComponents();
        }

        /// <summary>
        /// Destroy
        /// </summary>
        private void OnDestroy()
        {
            _disposables?.Dispose();
        }
        
        /// <summary>
        /// フェーズが切り替わったときの処理
        /// NOTE: フェーズマネージャーが取得出来なかったときは呼ばれない
        /// </summary>
        private void HandlePhaseChanged(PhaseEnum newPhase)
        {
            switch (newPhase)
            {
                // ムービー時はUI非表示
                case PhaseEnum.Movie:
                    BattleEnd();
                    break;
                
                // バトル中・チュートリアル中はUI表示
                case PhaseEnum.Battle:
                case PhaseEnum.Tutorial:
                    ShowBattleUI();
                break;
            }
        }

        /// <summary>
        /// コンポーネントの検証を行う
        /// </summary>
        private void ValidateComponents()
        {
            Debug.Assert(_canvasControllers != null && _canvasControllers.Length > 0, "canvasControllers が設定されていません");
            Debug.Assert(_scoreText != null, "scoreText が設定されていません");
            Debug.Assert(_finisherGuide != null, "finisherGuide が設定されていません");
            Debug.Assert(_chartRingManager != null, "warningIndicatorが設定されていません");
        }
        
        /// <summary>
        /// バトル開始時に関連するUIの表示処理を行う
        /// </summary>
        private void ShowBattleUI()
        {
            // 登録されているCanvasControllerのすべての表示処理を行う
            foreach (CanvasController canvasController in _canvasControllers)
            {
                canvasController.Show();
            }

            PrepareUIElements();
        }

        /// <summary>
        /// バトル終了時に関連するUIを非表示にする処理を行う
        /// </summary>
        private void BattleEnd()
        {
            // 登録されているCanvasControllerのすべての非表示処理を行う
            foreach (CanvasController canvasController in _canvasControllers)
            {
                canvasController.Hide();
            }
        }
        
        /// <summary>
        /// UI要素の準備
        /// </summary>
        private void PrepareUIElements()
        {
            _scoreText.SavePreBattleScore(); // バトル前の時点のスコアを保存する
            _finisherGuide.CountReset();
        }
    }
}
