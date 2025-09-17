using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    public class AisacControlAsset : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<AisacControlBehaviour>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.SetData(_aisacName, _duration, _start, _end);

            return playable;
        }

        [SerializeField]
        private string _aisacName;
        [SerializeField]
        private float _duration = 1;

        [SerializeField, Range(0, 1)]
        private float _start;
        [SerializeField, Range(0, 1)]
        private float _end;

    }
}
