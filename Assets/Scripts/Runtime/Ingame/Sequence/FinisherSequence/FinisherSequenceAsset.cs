using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class FinisherSequenceAsset : PlayableAsset
    {
        [SerializeField, Range(1, 2)] private int _behaviour;
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            switch (_behaviour)
            {
                case 1:
                    return SequenceBehaviourBase.CreatePlayable<FinisherSequenceBehaviour_1>(graph, owner);

                case 2:
                    return SequenceBehaviourBase.CreatePlayable<FinisherSequenceBehaviour_2>(graph, owner);
            }

            return Playable.Null;
        }
    }
}
