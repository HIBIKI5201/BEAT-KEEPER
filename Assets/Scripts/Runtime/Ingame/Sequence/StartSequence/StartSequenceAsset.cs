using System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    [Serializable]
    public class StartSequenceAsset : PlayableAsset
    {
        [SerializeField, Range(1, 3)] private int _behaviourKind;

        override public Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            switch (_behaviourKind)
            {
                case 1:
                    var playable1 = ScriptPlayable<StartSequenceBehaviour_1>.Create(graph);
                    var behaviour1 = playable1.GetBehaviour();
                    behaviour1.OnCreate(owner);

                    return playable1;

                case 2:
                    var playable2 = ScriptPlayable<StartSequenceBehaviour_2>.Create(graph);
                    var behaviour2 = playable2.GetBehaviour();
                    behaviour2.OnCreate(owner);

                    return playable2;

                case 3:
                    var playable3 = ScriptPlayable<StartSequenceBehaviour_3>.Create(graph);

                    return playable3;
            }

            return new Playable();
        }
    }
}
