using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    /// <summary>
    ///   ブレイクムービーシークエンスのPlayableAssetクラス
    /// </summary>
    public class BreakMovieSequenceAsset : PlayableAsset
    {
        [SerializeField, Range(1, 1)] private int _behaviour;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            switch (_behaviour)
            {
                case 1:
                    return ScriptPlayable<BreakMovieSequenceBehaviour_1>.Create(graph);
            }
            return Playable.Null;
        }
    }
}
