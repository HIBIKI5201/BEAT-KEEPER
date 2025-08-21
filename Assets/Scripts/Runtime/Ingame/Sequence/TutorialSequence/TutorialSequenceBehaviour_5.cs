using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Sequence;
using UnityEngine;
using UnityEngine.Playables;
using static UnityEngine.UI.GridLayoutGroup;

namespace BeatKeeper
{
    public class TutorialSequenceBehaviour_5 : SequenceBehaviourBase
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
            if (_owner)
            {
                TutorialManager tutorialManager = _owner.GetComponent<TutorialManager>();
                if (tutorialManager)
                {
                    tutorialManager.TutorialRegister(ChartKindEnum.Charge);
                }
            }
        }
    }
}
