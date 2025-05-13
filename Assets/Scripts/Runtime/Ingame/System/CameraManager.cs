using System;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.System;
using Unity.Cinemachine;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// カメラマネージャー
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera[] _camera;
        [SerializeField] private Transform _playerCameraTarget; // プレイヤーのカメラターゲット
        [SerializeField] private Transform[] _npcCameraTarget; // NPCのカメラターゲット（バトルごとにNPCが異なる予定なので、一旦配列で作成）
        [SerializeField] private Transform[] _enemyCameraTarget; // 次に出現する敵のカメラターゲット 
        private CinemachineCamera _useCamera;

        private void Start()
        {
             _playerCameraTarget = ServiceLocator.GetInstance<PlayerManager>().transform;
             ChangeCamera(CameraType.PlayerTPS);
        }

        /// <summary>
        /// プレイヤーとNPCのカメラを切り替える
        /// </summary>
        public void ChangeTarget(CameraAim targetName)
        {
            // 引数で指定されたターゲットにカメラを向ける
            Transform target = targetName switch
            {
                CameraAim.Player => _playerCameraTarget,
                CameraAim.NPC1 => _npcCameraTarget[0],
                CameraAim.FirstBattleEnemy => _enemyCameraTarget[0],
                CameraAim.SecondBattleEnemy => _enemyCameraTarget[1],
                _ => _playerCameraTarget
            };
            
            _useCamera.Follow = target;
        }

        /// <summary>
        /// 使用するカメラを変更する
        /// </summary>
        public void ChangeCamera(CameraType cameraType)
        {
            for (int i = 0; i < _camera.Length; i++)
            {
                if (i == (int)cameraType)
                {
                    _useCamera = _camera[i];
                    _camera[i].gameObject.SetActive(true);
                }
                else
                {
                    _camera[i].gameObject.SetActive(false);
                }
            }
        }
    }
    
    public enum CameraAim
    {
        Player,
        NPC1,
        NPC2,
        NPC3,
        FirstBattleEnemy,
        SecondBattleEnemy,
        ThirdBattleEnemy,
    }

    public enum CameraType
    {
        PlayerTPS,
        StartPerformance,
    }
}
