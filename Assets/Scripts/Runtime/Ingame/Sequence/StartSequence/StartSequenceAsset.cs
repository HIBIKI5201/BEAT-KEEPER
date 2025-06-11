using System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    /// <summary>
    ///     スタートシークエンスのPlayableAssetクラス
    /// </summary>
    public class StartSequenceAsset : PlayableAsset
    {
        [SerializeField, Range(1, 3)] private int _behaviourKind;

        override public Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            switch (_behaviourKind)
            {
                case 1:
                    return SequenceBehaviourBase.CreatePlayable<StartSequenceBehaviour_1>(graph, owner);

                case 2:
                    return SequenceBehaviourBase.CreatePlayable<StartSequenceBehaviour_2>(graph, owner);

                case 3:
                    return ScriptPlayable<StartSequenceBehaviour_3>.Create(graph);
            }

            return Playable.Null;
        }
    }
}
