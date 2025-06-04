using BeatKeeper.Runtime.Ingame.Stsge;
using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.Ingame.UI;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class StartPerformanceBehaviour_2 : PlayableBehaviour
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
            
            var cameraManager = ServiceLocator.GetInstance<StageManager>().CameraManager;
            cameraManager.ChangeCamera(0);
            var uiManager = ServiceLocator.GetInstance<InGameUIManager>();
            uiManager.HideEncounterText();
            uiManager.BattleStart();
            var bgmChanger = ServiceLocator.GetInstance<BGMChanger>();
            bgmChanger.ChangeBGM("Battle1");
            
            Debug.Log("StartPerformanceBehaviour_2");
        }
    }
}
