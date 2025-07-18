﻿using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    /// <summary>
    ///     InGameシーンのマネージャー
    /// </summary>
    public class InGameSceneManager : SceneManagerB
    {
        private void Start()
        {
            var multiScene = ServiceLocator.GetInstance<MultiSceneManager>();
            multiScene.SceneLoad(SceneListEnum.Stage);
            multiScene.SceneLoad(SceneListEnum.Battle);

            SceneLoader.RegisterAfterSceneLoad(SceneListEnum.Stage.ToString(),
                                ActiveStageScene);

            async void ActiveStageScene()
            {
                await Awaitable.EndOfFrameAsync();
                SceneLoader.SetActiveScene(SceneListEnum.Stage.ToString());
            }
        }
    }
}
