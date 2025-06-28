using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Playables;
using BeatKeeper.Runtime.Ingame.System;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class StartSequenceBehaviour_3 : PlayableBehaviour
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);

            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            if (phaseManager)
            {
                phaseManager.NextPhase();
            }

            Debug.Log("StartPerformanceBehaviour_3");
        }
    }
}
