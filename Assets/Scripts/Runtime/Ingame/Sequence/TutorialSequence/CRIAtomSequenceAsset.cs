using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{

    [System.Serializable]
    public class CriAtomSequenceAsset : PlayableAsset
    {
        [SerializeField] private string _cueName;  // Timeline上で設定するキュー名
        [SerializeField] private string _text;    // Timeline上で設定するテキスト

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<CriAtomSequenceBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.CueName = _cueName;
            behaviour.Text = _text;
            behaviour.Owner = owner;
            return playable;
        }
    }

}
