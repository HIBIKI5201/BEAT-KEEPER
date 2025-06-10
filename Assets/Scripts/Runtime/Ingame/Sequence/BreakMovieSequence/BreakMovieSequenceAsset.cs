using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class BreakMovieSequenceAsset : PlayableAsset
    {
        [SerializeField, Range(1, 1)] private int _behaviour;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            switch (_behaviour)
            {
                case 1:
                    var playable = ScriptPlayable<BreakMovieSequenceBehaviour_1>.Create(graph);
                    return playable;
            }
            return Playable.Null;
        }
    }
}
