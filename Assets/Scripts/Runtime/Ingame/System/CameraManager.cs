using System;
using System.Collections.Generic;
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
        private CinemachineCamera _camera;

        private readonly Dictionary<string, CinemachineCamera> _cameras = new ();

        private async void Start()
        {
            var player = await ServiceLocator.GetInstanceAsync<PlayerManager>();

            await SceneLoader.WaitForLoadSceneAsync("Battle"); // バトルシーンが読み込まれるまで待機する

            ChangeCamera(player.PlayerCamera);
        }

        /// <summary>
        ///     使用するカメラを変更する
        /// </summary>
        public void ChangeCamera(CinemachineCamera camera)
        {
            if (!camera)
            {
                Debug.LogWarning("[CameraManager] camera is null");
                return;
            }

            if (_camera) _camera.enabled = false;
            camera.enabled = true;

            _camera = camera;
        }
    }
}
