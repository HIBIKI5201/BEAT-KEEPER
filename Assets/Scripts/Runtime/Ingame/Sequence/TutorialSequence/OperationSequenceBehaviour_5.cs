using BeatKeeper.Runtime.Ingame.Sequence;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class OperationSequenceBehaviour_5 : SequenceBehaviourBase
    {
        private bool _isFirstFrame = true;
        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
            if (_owner && _isFirstFrame)
            {
                _isFirstFrame = false;
                _owner.GetComponent<TutorialManager>()?.PauseTimeline();
            }
        }
    }
}
