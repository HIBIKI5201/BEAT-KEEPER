using BeatKeeper.Runtime.Ingame.Sequence;
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
            if (_owner)
            {
                var resultManager = _owner.GetComponent<ResultManager>();
                if (resultManager)
                {
                    resultManager.ResultShow();
                }
            }
        }
    }
}
