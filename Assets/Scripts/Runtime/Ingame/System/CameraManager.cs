using System;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace BeatKeeper
{
    /// <summary>
    /// カメラマネージャー
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
        private CinemachineBrain _brain;
        
        private CinemachineCamera _playerCamera;
        
        [FormerlySerializedAs("_camera")] [SerializeField] private CinemachineCamera[] _cameras;
        private int _index;

        private void Start()
        {
            _brain = Camera.main?.GetComponent<CinemachineBrain>();
            
            var player = ServiceLocator.GetInstance<PlayerManager>();
            _playerCamera = player.GetComponentInChildren<CinemachineCamera>();
            _cameras[0] = _playerCamera;
            
            ChangeCamera(0);
        }

        /// <summary>
        /// 使用するカメラを変更する
        /// </summary>
        public void ChangeCamera(int index)
        {
            _cameras[_index].enabled = false;
            _cameras[index].enabled = true;
            _index = index;
        }
    }

    public enum CameraType
    {
        Player = 0,
        StartPerformance = 1,
    }
}
