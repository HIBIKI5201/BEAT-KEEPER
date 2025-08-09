using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Sequence;
using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class TutorialSequenceBehaviour_Fin : SequenceBehaviourBase
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
            if (_owner)
            {
                var tutorialManager = _owner.GetComponent<TutorialManager>();

            }
        }
    }
}
