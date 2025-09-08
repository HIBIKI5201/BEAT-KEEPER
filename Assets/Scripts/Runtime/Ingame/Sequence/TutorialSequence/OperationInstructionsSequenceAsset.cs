using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Sequence;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class OperationInstructionsSequenceAsset : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            switch (_tutorialPhase)
            {
                case 1:
                    return SequenceBehaviourBase.CreatePlayable<OperationSequenceBehaviour_1>(graph, owner);
                case 2:
                    return SequenceBehaviourBase.CreatePlayable<OperationSequenceBehaviour_2>(graph, owner);
                case 3:
                    return SequenceBehaviourBase.CreatePlayable<OperationSequenceBehaviour_3>(graph, owner);
                case 4:
                    return SequenceBehaviourBase.CreatePlayable<OperationSequenceBehaviour_4>(graph, owner);
                case 5:
                    return SequenceBehaviourBase.CreatePlayable<OperationSequenceBehaviour_5>(graph, owner);
                default:
                    return Playable.Null;
            }
        }

        [SerializeField, Range(1, 6)] private int _tutorialPhase;
    }
}

