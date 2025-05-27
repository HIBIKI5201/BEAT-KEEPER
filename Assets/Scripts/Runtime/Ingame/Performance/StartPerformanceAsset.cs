using System;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper
{
    [Serializable]
    public class StartPerformanceAsset : PlayableAsset
    {
        private enum BehaviourKind
        {
            Behaviour1,
            Behaviour2,
            Behaviour3,
        }
        
        [SerializeField] private BehaviourKind _behaviourKind;
        
        override public Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            Playable playable = _behaviourKind switch
            {
                BehaviourKind.Behaviour1 => ScriptPlayable<StartPerformanceBehaviour_1>.Create(graph),
                BehaviourKind.Behaviour2 => ScriptPlayable<StartPerformanceBehaviour_2>.Create(graph),
                BehaviourKind.Behaviour3 => ScriptPlayable<StartPerformanceBehaviour_3>.Create(graph),
                _ => new Playable()
            };
            return playable;
        }
    }
}
