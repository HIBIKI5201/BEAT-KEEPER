using System.Collections.Generic;
using System.Threading.Tasks;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    public class MultiSceneManager : MonoBehaviour
    {
        private Dictionary<SceneListEnum, bool> _sceneLoadProgress;

        public void SceneLoad(SceneListEnum scene)
        {
            SceneLoader.LoadScene(scene.ToString());

            if (!_sceneLoadProgress.TryAdd(scene, false))
            {
                Debug.LogWarning($"failed to load scene : {scene}");
            }
        }
        
        public bool GetSceneLoadProgress(SceneListEnum scene) => _sceneLoadProgress[scene];

        /// <summary>
        /// シーンのロードが終わるまで待機
        /// </summary>
        /// <param name="scene"></param>
        public async Task WaitForSceneLoad(SceneListEnum scene)
        {
            if (!_sceneLoadProgress.ContainsKey(scene))
            {
                Debug.LogWarning($"{scene} is not loaded");
                return;
            }
            
            while (GetSceneLoadProgress(scene)) //シーンのロードが終わるまで待機
            {
                await PauseManager.PausableNextFrameAsync(destroyCancellationToken);
            }
        }
    }
}
