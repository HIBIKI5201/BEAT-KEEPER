using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Sequence;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class TutorialSequenceBehaviour_1 : SequenceBehaviourBase
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            Debug.Log("TutorialSequenceActivate");
            base.OnBehaviourPlay(playable, info);
            if (_owner)
            {
                TutorialManager tutorialManager = _owner.GetComponent<TutorialManager>();
                if (tutorialManager)
                {
                    tutorialManager.TutorialRegister(ChartKindEnum.Attack);
                }
                else
                {
                    Debug.LogError("TutorialSequenceBehaviour_1: TutorialManager component not found on owner.");
                }
            }
            else
            {
                Debug.LogError("TutorialSequenceBehaviour_1: Owner is null. Cannot register tutorial.");
            }
        }
    }
}
