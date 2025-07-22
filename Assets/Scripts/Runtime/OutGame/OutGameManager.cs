using Cysharp.Threading.Tasks;
using SymphonyFrameWork.System;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// 
    /// </summary>
    public class OutGameManager : MonoBehaviour
    {
        private const string OutGameSceneName = "OutGame";
        private const string InGameSceneName = "InGame";
        private const string StageSceneName = "Stage";

        private async void Awake()
        {
            await SceneLoader.LoadScene(StageSceneName);
            //await LoadInGameSceneAsync();
        }

        /// <summary>
        /// インゲームシーンを読み込むメソッド
        /// </summary>
        /// <returns></returns>
        private async Task LoadInGameSceneAsync()
        {
            await SceneLoader.UnloadScene(OutGameSceneName);
            await SceneLoader.LoadScene(InGameSceneName);
            SceneLoader.SetActiveScene(InGameSceneName);
        }
        
        public async void LoadInGameScene()
        {
            await LoadInGameSceneAsync();
        }
    }
}
