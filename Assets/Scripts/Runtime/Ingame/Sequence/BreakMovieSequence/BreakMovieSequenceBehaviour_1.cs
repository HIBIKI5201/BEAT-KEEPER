using BeatKeeper.Runtime.Ingame.UI;
using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class BreakMovieSequenceBehaviour_1 : PlayableBehaviour
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            var phaseManager = ServiceLocator.GetInstance<UIElement_CountDown>();
            if (phaseManager)
            {
                phaseManager.Play();
            }
        }
    }
}
