using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class BGMChangeAsset : PlayableAsset
    {
        [SerializeField]
        private string _bgmName = string.Empty;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<BGMChangeBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.SetBGMName(_bgmName);

            return playable;
        }
    }
}
