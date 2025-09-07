using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.System.Sequence
{
    public class BGMChangeAsset : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<BGMChangeBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.SetBGMName(_bgmName);

            return playable;
        }

        [SerializeField]
        private string _bgmName = string.Empty;
    }
}
