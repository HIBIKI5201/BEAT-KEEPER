using Unity.Cinemachine;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// カメラマネージャー
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _camera;
        [SerializeField] private Transform _playerCameraTarget; // プレイヤーのカメラターゲット
        [SerializeField] private Transform[] _npcCameraTarget; // NPCのカメラターゲット（バトルごとにNPCが異なる予定なので、一旦配列で作成）
        [SerializeField] private Transform[] _nextEnemyCameraTarget; // 次に出現する敵のカメラターゲット 
        
        /// <summary>
        /// プレイヤーとNPCのカメラを切り替える
        /// </summary>
        public void CameraChange(CameraAim targetName)
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
    }
    
    public enum CameraAim
    {
        Player,
        NPC1,
        NPC2,
        NPC3,
        SecondBattleEnemy,
        ThirdBattleEnemy,
    }
}
