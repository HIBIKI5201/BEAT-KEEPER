using UnityEngine;
using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using R3;

namespace BeatKeeper
{
    /// <summary>
    /// コンボUIの表示・非表示を操作するためのクラス
    /// </summary>
    [RequireComponent(typeof(CanvasController))]
    public class ComboTextCanvasController : MonoBehaviour
    {
        private CanvasController _canvasController;
        
        // ムービーの時にUIを隠す処理を行うためのフェーズマネージャーの参照
        private PhaseManager _phaseManager;
        private CompositeDisposable _disposables = new CompositeDisposable();
        
        private bool _hasComboCount; // コンボ中か

        #region Life cycle

        /// <summary>
        /// Start
        /// </summary>
        private async void Start()
        {
            _canvasController = GetComponent<CanvasController>();
            
            // 非表示の位置にずれた状態で始まってしまうので、デフォルトの位置に戻す
            _canvasController.SetDefaultPosition();
            
            _phaseManager = await ServiceLocator.GetInstanceAsync<PhaseManager>();
            if (_phaseManager != null)
            {
                // ムービー中にはUIが表示されないようにしたいので、フェーズのリアクティブプロパティを購読する
                _phaseManager.CurrentPhaseProp.Subscribe(HandlePhaseChanged).AddTo(_disposables);
            }
            else
            {
                Debug.LogError("フェーズマネージャーが取得できませんでした。ムービー中のコンボUI非表示処理が行われません");
            }
        }
        
        /// <summary>
        /// Destroy
        /// </summary>
        private void OnDestroy()
        {
            _disposables?.Dispose();
        }

        #endregion

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
                    Hide();
                    break;
                
                // バトル中・チュートリアル中はUI表示
                case PhaseEnum.Battle:
                case PhaseEnum.Tutorial:
                    Show();
                    break;
            }
        }

        /// <summary>
        /// 表示
        /// </summary>
        private void Show()
        {
            if(_canvasController == null) return;
            
            if (_hasComboCount)
            {
                // コンボ継続中であればコンボUIを表示する
                _canvasController.Show();
            }
        }

        /// <summary>
        /// 非表示
        /// </summary>
        private void Hide()
        {
            if(_canvasController == null) return;
            
            if (_canvasController.CanvasGroup.alpha >= 1)
            {
                // CanvasControllerのa値が1 = 表示中であればコンボ継続中ということなので
                // ムービー後に再度スライドインできるようにフラグを立てて、非表示処理を行う
                _hasComboCount = true;
                _canvasController.Hide();
            }
            else
            {
                _hasComboCount = false;
            }
        }
    }
}
