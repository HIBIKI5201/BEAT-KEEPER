using BeatKeeper.Runtime.Ingame.Sequence;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class ResultSequenceBehaviour_1 : SequenceBehaviourBase
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            Debug.Log("ResultSequenceActivate");
            base.OnBehaviourPlay(playable, info);

            ResultManager resultManager = ServiceLocator.GetInstance<ResultManager>();

            if (resultManager)
            {
                resultManager.ResultShow();
            }
        }
    }
}
