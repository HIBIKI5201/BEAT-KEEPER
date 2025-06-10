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
                    var playable1 = ScriptPlayable<FinisherSequenceBehaviour_1>.Create(graph);
                    var behaviour1 = playable1.GetBehaviour();
                    behaviour1.OnCreate(owner);

                    return playable1;

                case 2:
                    var playable2 = ScriptPlayable<FinisherSequenceBehaviour_2>.Create(graph);
                    var behaviour2 = playable2.GetBehaviour();
                    behaviour2.OnCreate(owner);

                    return playable2;
            }

            return Playable.Null;
        }
    }
}
