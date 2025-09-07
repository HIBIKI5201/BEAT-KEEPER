using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Sequence;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class OperationSequenceBehaviour_1 : SequenceBehaviourBase
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
            if (_owner)
            {
                TutorialManager tutorialManager = _owner.GetComponent<TutorialManager>();
                if (tutorialManager)
                {
                    tutorialManager.OperationTutorialStart(ChartKindEnum.Attack);
                }
            }
        }
    }
}
