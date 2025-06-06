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
            switch(_behaviourKind)
            {
                case BehaviourKind.Behaviour1:
                    var playable1 = ScriptPlayable<StartPerformanceBehaviour_1>.Create(graph);
                    var behaviour1 = playable1.GetBehaviour();

                    behaviour1.OnCreate(owner);
                    return playable1;

                case BehaviourKind.Behaviour2:
                    var playable2 = ScriptPlayable<StartPerformanceBehaviour_2>.Create(graph);
                    return playable2;

                    case BehaviourKind.Behaviour3:
                    var playable3 = ScriptPlayable<StartPerformanceBehaviour_3>.Create(graph);
                    
                    return playable3;
            }

            return new Playable();
        }
    }
}
