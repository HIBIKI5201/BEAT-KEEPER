using BeatKeeper.Runtime.Ingame.Sequence;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class ResultSequenceBehaviour_2 : SequenceBehaviourBase
    {
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {

            base.OnBehaviourPlay(playable, info);
            if (_owner)
            {
                ResultManager resultManager = _owner.GetComponent<ResultManager>();
                if (resultManager)
                {
                    resultManager.AllProductionCompleted();
                }
                else
                {
                                       Debug.LogError("ResultSequenceBehaviour_2: ResultManager component not found on owner.");
                }
            }
        }
    }
}
