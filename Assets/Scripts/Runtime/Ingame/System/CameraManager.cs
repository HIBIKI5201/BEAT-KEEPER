using BeatKeeper.Runtime.Ingame.Stsge;
using SymphonyFrameWork.System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

namespace BeatKeeper.Runtime.System
{
    /// <summary>
    /// カメラマネージャー
    /// </summary>
    public class CameraManager : MonoBehaviour
    {
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

        private CinemachineCamera _camera;
        private readonly Dictionary<string, CinemachineCamera> _cameras = new();

        private async void Start()
        {
            var stageManager = await ServiceLocator.GetInstanceAsync<StageSceneManager>();

            ChangeCamera(stageManager.PlayerCamera);
        }
    }
}
