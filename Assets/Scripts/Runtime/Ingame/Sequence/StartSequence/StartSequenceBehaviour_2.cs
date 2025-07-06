using BeatKeeper.Runtime.Ingame.Stsge;
using BeatKeeper.Runtime.Ingame.UI;
using BeatKeeper.Runtime.System;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class StartSequenceBehaviour_2 : SequenceBehaviourBase
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);

            //カメラをプレイヤーに変更
            var cameraManager = ServiceLocator.GetInstance<CameraManager>();
            if (cameraManager)
            {
                var stageManager = ServiceLocator.GetInstance<StageSceneManager>();
                cameraManager.ChangeCamera(stageManager.PlayerCamera);
            }

            //スタートテキストを隠す
            var text = _owner.GetComponentInChildren<UIElement_EncounterText>();
            if (text)
            {
                text.HideEncounterText();
            }

            //バトルHUDを表示
            var uiManager = ServiceLocator.GetInstance<InGameUIManager>();
            if (uiManager)
            {
                uiManager.BattleStart();
            }

            Debug.Log("StartPerformanceBehaviour_2");
        }
    }
}
