using BeatKeeper.Runtime.Ingame.UI;
using SymphonyFrameWork.System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class StartPerformanceBehaviour_1 : PlayableBehaviour
    {
        private GameObject _owner;

        public void OnCreate(GameObject owner)
        {
            _owner = owner;
        }

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);

            if (_owner)
            {
                var camera = _owner.GetComponentInChildren<CinemachineCamera>();
                if (camera)
                {
                    var cameraManager = ServiceLocator.GetInstance<CameraManager>();
                    if (cameraManager)
                        cameraManager.ChangeCamera(camera);
                }
            }

            var text = _owner.GetComponentInChildren<UIElement_EncounterText>();
            if (text)
            {
                text.ShowEncounterText(1);
            }

            Debug.Log("StartPerformanceBehaviour_1");
        }
    }
}
