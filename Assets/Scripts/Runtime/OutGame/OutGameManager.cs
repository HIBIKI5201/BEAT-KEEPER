using Cysharp.Threading.Tasks;
using SymphonyFrameWork.System;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatKeeper
{
    public class OutGameManager : MonoBehaviour
    {
        private const string OutGameSceneName = "OutGame";
        private const string InGameSceneName = "InGame";
        private const string StageSceneName = "Stage";
        public async void Awake()
        {
            await LoadSceneAsync(StageSceneName);
            //await LoadInGameSceneAsync();
        }

        /// <summary>
        /// インゲームシーンを読み込むメソッド
        /// </summary>
        /// <returns></returns>
        async Task LoadInGameSceneAsync()
        {
            await SceneLoader.UnloadScene(OutGameSceneName);
            await SceneLoader.LoadScene(InGameSceneName);
            SceneLoader.SetActiveScene(InGameSceneName);
        }

        /// <summary>
        /// 文字列で指定したシーンを非同期で読み込むメソッド
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns></returns>
        async Task LoadSceneAsync(string sceneName)
        {
            // シーンの非同期読み込み
            await SceneLoader.LoadScene(sceneName);
        }
    }
}
