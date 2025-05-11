using SymphonyFrameWork.System;
using Unity.Cinemachine;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// クリアフェーズ全体の流れを管理するマネージャークラス
    /// </summary>
    public class ClearPhaseManager : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _camera;
        [SerializeField] private CameraManager _cameraManager;
        [SerializeField] private BattleResultController _battleResultController;
        
        private PhaseManager _phaseManager;
        private MusicEngineHelper _musicEngineHelper;
        private int _count;
        
        private void Start()
        {
            _phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            _musicEngineHelper = ServiceLocator.GetInstance<MusicEngineHelper>();
            _battleResultController.Hide(); // 最初は表示しないようにする
        }

        /// <summary>
        /// クリアフェーズが始まったら呼ばれる処理
        /// </summary>
        public void ClearPhaseStart()
        {
            _musicEngineHelper.OnJustChangedBar += Counter;
        }
        
        /// <summary>
        /// 拍数を元に遷移処理を行う
        /// </summary>
        private void Counter()
        {
            _count++;
            if (_count == 1)
            {
                // リザルト表示、NPCにフォーカス。NPCが褒めてくれる演出
                ShowBattleResult();
                _cameraManager.CameraChange(CameraAim.NPC1);
            }
            else if (_count == 5)
            {
                // リザルト表示を隠す
                HideBattleResult();
            }
            else if (_count == 9)
            {
                // 次の敵が出現（NPCを追いかけている状態）。カメラを向ける
                _cameraManager.CameraChange(CameraAim.SecondBattleEnemy);
            }
            else if (_count == 13)
            {
                // プレイヤーにカメラを戻して、武器を構えるモーション
                _cameraManager.CameraChange(CameraAim.Player);
            }
            else if (_count == 17)
            {
                // バトルフェーズを始める
                ActivateBattlePhase();
            }
        }
        
        /// <summary>
        /// バトルリザルトを表示する
        /// </summary>
        private void ShowBattleResult()
        {
            _battleResultController.Show();
        }

        /// <summary>
        /// バトルリザルトを非表示にする
        /// </summary>
        private void HideBattleResult()
        {
            _battleResultController.Hide();
        }
        
        /// <summary>
        /// バトルフェーズに移行する
        /// </summary>
        private void ActivateBattlePhase()
        {
            _phaseManager.TransitionTo(PhaseEnum.Battle2); // TODO: PhaseManager側に、次のシーンを再生する仕組みを追加する
            _musicEngineHelper.OnJustChangedBar -= Counter; // 購読を解除する
        }
        
        private void OnDestroy()
        {
            _musicEngineHelper.OnJustChangedBar -= Counter;
        }
    }
}
