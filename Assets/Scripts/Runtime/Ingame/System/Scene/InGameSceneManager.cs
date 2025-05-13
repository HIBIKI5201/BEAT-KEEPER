using System;
using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper
{
    public class InGameSceneManager : SceneManagerB
    {
        private void Start()
        {
            ServiceLocator.GetInstance<MultiSceneManager>().SceneLoad(SceneListEnum.InGame);
        }
    }
}
