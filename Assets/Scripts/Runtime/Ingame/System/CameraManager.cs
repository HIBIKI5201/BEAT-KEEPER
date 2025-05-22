using System;
using BeatKeeper.Runtime.Ingame.Battle;
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
        private CinemachineCamera _playerCamera;
        [SerializeField] private CinemachineCamera[] _cameras;
        private int _index;

        private async void Start()
        {
            var player = ServiceLocator.GetInstance<PlayerManager>();
            _playerCamera = player.GetComponentInChildren<CinemachineCamera>();
            _cameras[0] = _playerCamera; // 仮 プレイヤーの子オブジェクトのカメラを使用する
            
            await SceneLoader.WaitForLoadSceneAsync("Battle"); // バトルシーンが読み込まれるまで待機する
            
            _cameras[0].Follow = player.transform;
            _cameras[0].LookAt = ServiceLocator.GetInstance<BattleSceneManager>().EnemyAdmin.Enemies[0].transform;
            
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
}
