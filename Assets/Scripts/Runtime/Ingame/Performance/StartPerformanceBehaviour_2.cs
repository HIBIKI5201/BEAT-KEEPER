using BeatKeeper.Runtime.Ingame.Character;
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
            
            var cameraManager = ServiceLocator.GetInstance<CameraManager>();
            if (cameraManager)
            {
                var player = ServiceLocator.GetInstance<PlayerManager>();
                cameraManager.ChangeCamera(player.PlayerCamera);
            }

            var uiManager = ServiceLocator.GetInstance<InGameUIManager>();
            if (uiManager)
            {
                uiManager.HideEncounterText();
                uiManager.BattleStart();
            }

            var bgmChanger = ServiceLocator.GetInstance<BGMChanger>();
            if (bgmChanger)
            {
                bgmChanger.ChangeBGM("Battle1");
            }

            Debug.Log("StartPerformanceBehaviour_2");
        }
    }
}
