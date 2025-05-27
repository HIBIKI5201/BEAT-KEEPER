using BeatKeeper.Runtime.Ingame.Stsge;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class StartPerformanceBehaviour_1 : PlayableBehaviour
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
            
            var cameraManager = ServiceLocator.GetInstance<StageManager>().CameraManager;
            cameraManager.ChangeCamera(1);
            var uiManager = ServiceLocator.GetInstance<InGameUIManager>();
            uiManager.ShowEncounterText(1);
            
            Debug.Log("StartPerformanceBehaviour_1");
        }
    }
}
