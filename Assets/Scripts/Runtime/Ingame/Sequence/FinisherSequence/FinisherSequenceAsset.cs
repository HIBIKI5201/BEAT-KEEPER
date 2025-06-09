using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    public class FinisherSequenceAsset : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable1 = ScriptPlayable<FinisherSequenceBehaviour_1>.Create(graph);
            var behaviour1 = playable1.GetBehaviour();
            behaviour1.OnCreate(owner);

            return playable1;
        }
    }
}
