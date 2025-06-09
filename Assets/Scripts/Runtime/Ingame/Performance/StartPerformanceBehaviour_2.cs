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
        private GameObject _owner;

        public void OnCreate(GameObject owner)
        {
            _owner = owner;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
            
            //カメラをプレイヤーに変更
            var cameraManager = ServiceLocator.GetInstance<CameraManager>();
            if (cameraManager)
            {
                var player = ServiceLocator.GetInstance<PlayerManager>();
                cameraManager.ChangeCamera(player.PlayerCamera);
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

            //BGMをバトルに変更
            var bgmChanger = ServiceLocator.GetInstance<BGMChanger>();
            if (bgmChanger)
            {
                bgmChanger.ChangeBGM("Battle1");
            }

            Debug.Log("StartPerformanceBehaviour_2");
        }
    }
}
