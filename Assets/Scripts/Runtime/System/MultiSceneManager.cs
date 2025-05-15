using System.Collections.Generic;
using System.Threading.Tasks;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    public class MultiSceneManager : MonoBehaviour
    {
        private Dictionary<SceneListEnum, bool> _sceneLoadProgress = new();

        public async void SceneLoad(SceneListEnum scene)
        {
            var task = SceneLoader.LoadScene(scene.ToString());

            if (!_sceneLoadProgress.TryAdd(scene, false))
            {
                Debug.LogWarning($"failed to load scene : {scene}");
                return;
            }
            
            await PauseManager.PausableWaitUntil(() => task.IsCompleted, destroyCancellationToken);
            
            _sceneLoadProgress[scene] = true;
        }

        public bool GetSceneLoadProgress(SceneListEnum scene)
        {
            if (_sceneLoadProgress.TryGetValue(scene, out var result))
                return result; 
            
            return true;
        }

        /// <summary>
        /// シーンのロードが終わるまで待機
        /// </summary>
        /// <param name="scene"></param>
        public async Task WaitForSceneLoad(SceneListEnum scene)
        {
            if (!_sceneLoadProgress.ContainsKey(scene))
            {
                Debug.LogWarning($"{scene} is not loaded");
            }
            
            while (!GetSceneLoadProgress(scene)) //シーンのロードが終わるまで待機
            {
                await PauseManager.PausableNextFrameAsync(destroyCancellationToken);
            }
        }
    }
}
