using System;
using BeatKeeper.Runtime.Ingame.Character;
using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper
{
    public class InGameSceneManager : SceneManagerB
    {
        private void Start()
        {
            var multiScene = ServiceLocator.GetInstance<MultiSceneManager>();
            multiScene.SceneLoad(SceneListEnum.Stage);
            multiScene.SceneLoad(SceneListEnum.Battle);
        }
    }
}
