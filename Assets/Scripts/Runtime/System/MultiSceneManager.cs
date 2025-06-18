using System.Collections.Generic;
using System.Threading.Tasks;
using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    public class MultiSceneManager : MonoBehaviour
    {
        private Dictionary<SceneListEnum, bool> _loadedScenes = new();

        public async void SceneLoad(SceneListEnum sceneEnum)
        {
            var task = SceneLoader.LoadScene(sceneEnum.ToString());

            if (!_loadedScenes.TryAdd(sceneEnum, false))
            {
                Debug.LogWarning($"failed to load scene : {sceneEnum}");
                return;
            }
            
            await PauseManager.PausableWaitUntil(() => task.IsCompleted, destroyCancellationToken);
            
            if (SceneLoader.GetExistScene(sceneEnum.ToString(), out var scene))
            {
                //シーンのルートオブジェクトの非同期初期化を行う
                var rootObjects = scene.GetRootGameObjects();
                List<Task> tasks = new();

                for(int i = 0; i < rootObjects.Length; i++)
                {
                    if (rootObjects[i]
                        .TryGetComponent<IInitializeAsync>(out var initializeAsync))
                    {
                         tasks.Add(initializeAsync.DoInitialize());
                    }
                }

                await Task.WhenAll(tasks);
            }

            _loadedScenes[sceneEnum] = true;
        }

        public bool GetSceneLoadProgress(SceneListEnum scene)
        {
            if (_loadedScenes.TryGetValue(scene, out var result))
                return result; 
            
            return true;
        }

        /// <summary>
        /// シーンのロードが終わるまで待機
        /// </summary>
        /// <param name="scene"></param>
        public async Task WaitForSceneLoad(SceneListEnum scene)
        {
            if (!_loadedScenes.ContainsKey(scene))
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
