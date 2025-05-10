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
        [SerializeField] private BattleResultController _battleResultController;
        [SerializeField] private Transform _playerCameraTarget; // プレイヤーのカメラターゲット
        [SerializeField] private Transform[] _npcCameraTarget; // NPCのカメラターゲット（バトルごとにNPCが異なる予定なので、一旦配列で作成）
        [SerializeField] private Transform[] _nextEnemyCameraTarget; // 次に出現する敵のカメラターゲット 
        
        private PhaseManager _phaseManager;
        private MusicEngineHelper _musicEngineHelper;
        private int _count;
        
        private void Start()
        {
            _phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            _musicEngineHelper = ServiceLocator.GetInstance<MusicEngineHelper>();
            _battleResultController.Hide(); // 最初は表示しないようにする
            
            ClearPhaseStart(); // TODO: 現在テスト用にStart関数から呼んでいるが、フィニッシャー終了時に呼ぶようにしてほしい
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
                CameraChange(CameraAim.NPC1);
            }
            else if (_count == 5)
            {
                // リザルト表示を隠す
                HideBattleResult();
            }
            else if (_count == 9)
            {
                // 次の敵が出現（NPCを追いかけている状態）。カメラを向ける
                CameraChange(CameraAim.SecondBattleEnemy);
            }
            else if (_count == 13)
            {
                // プレイヤーにカメラを戻して、武器を構えるモーション
                CameraChange(CameraAim.Player);
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
        /// プレイヤーとNPCのカメラを切り替える
        /// </summary>
        private void CameraChange(CameraAim targetName)
        {
            // 引数で指定されたターゲットにカメラを向ける
            Transform target = targetName switch
            {
                CameraAim.Player => _playerCameraTarget,
                CameraAim.NPC1 => _npcCameraTarget[0],
                CameraAim.SecondBattleEnemy => _nextEnemyCameraTarget[0],
            };
            
            _camera.Follow = target;
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
        
        private enum CameraAim
        {
            Player,
            NPC1,
            NPC2,
            NPC3,
            SecondBattleEnemy,
            ThirdBattleEnemy,
        }
    }
}
