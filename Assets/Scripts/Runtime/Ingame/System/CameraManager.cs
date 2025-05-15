using System;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
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
        private CinemachineBrain _brain;
        
        private CinemachineCamera _playerCamera;
        
        [SerializeField] private CinemachineCamera[] _camera;
        [SerializeField] private Transform _playerCameraTarget; // プレイヤーのカメラターゲット
        [SerializeField] private Transform[] _npcCameraTarget; // NPCのカメラターゲット（バトルごとにNPCが異なる予定なので、一旦配列で作成）
        [SerializeField] private Transform[] _enemyCameraTarget; // 次に出現する敵のカメラターゲット 
        private CinemachineCamera _useCamera;

        private void Start()
        {
            _brain = Camera.main?.GetComponent<CinemachineBrain>();
            
            var player = ServiceLocator.GetInstance<PlayerManager>();
            _playerCamera = player.GetComponentInChildren<CinemachineCamera>();
            _playerCameraTarget = player.transform;
            
            ChangeCamera(CameraType.StartPerformance);
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
                CameraAim.Npc => _npcCameraTarget[0],
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
            switch (cameraType) //特定のカメラの時はそれをアクティブにする
            {
                case CameraType.PlayerTPS:
                    foreach (var cinemachineCamera in _camera)
                    cinemachineCamera.enabled = false;
                    _playerCamera.enabled = true;
                    return;
            }
            
            for (int i = 0; i < _camera.Length; i++)
            {
                if (i == (int)cameraType)
                {
                    _useCamera = _camera[i];
                    _camera[i].enabled = true;
                }
                else
                {
                    _camera[i].enabled = false;
                }
            }
        }
    }
    
    public enum CameraAim
    {
        Player,
        Npc,
        FirstBattleEnemy,
        SecondBattleEnemy,
        ThirdBattleEnemy,
    }

    public enum CameraType
    {
        StartPerformance = 0,
        PlayerTPS = 1,
    }
}
