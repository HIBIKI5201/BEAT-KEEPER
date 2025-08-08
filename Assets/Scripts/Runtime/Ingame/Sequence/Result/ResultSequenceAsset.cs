using BeatKeeper.Runtime.Ingame.Sequence;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class ResultSequenceAsset : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
             switch(_behaviour)
             {
                 case 1:
                    return SequenceBehaviourBase.CreatePlayable<ResultSequenceBehaviour_1>(graph, owner);
                 case 2:
                     return SequenceBehaviourBase.CreatePlayable<ResultSequenceBehaviour_2>(graph, owner);
            }
            return Playable.Null;
        }

        [SerializeField, Range(1, 2)] private int _behaviour;
    }
}
