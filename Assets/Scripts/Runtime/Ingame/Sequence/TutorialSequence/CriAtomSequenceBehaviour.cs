using UnityEngine;
using CriWare;
using UnityEngine;
using UnityEngine.Playables;
using BeatKeeper.Runtime.System;
using BeatKeeper.Runtime.Ingame.Sequence;
namespace BeatKeeper
{

    public class CriAtomSequenceBehaviour : PlayableBehaviour
    {
        public string CueName;
        public GameObject Owner;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            if (string.IsNullOrEmpty(CueName)) return;
            if (Owner)
            {
                var tutorialManager = Owner.GetComponent<TutorialManager>();
                if (tutorialManager)
                {
                    tutorialManager.PlayVoice(CueName);
                }
            }
        }
    }

}
