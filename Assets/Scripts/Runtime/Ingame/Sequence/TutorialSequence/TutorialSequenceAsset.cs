using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.UI;
using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    /// <summary>
    /// チュートリアルシークエンスのPlayableAssetクラス
    /// </summary>
    public class TutorialSequenceAsset : PlayableAsset
    {
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            switch (_tutorialPhase)
            {
                case 1:
                    return SequenceBehaviourBase.CreatePlayable<TutorialSequenceBehaviour_1>(graph, owner);
                case 2:
                    return SequenceBehaviourBase.CreatePlayable<TutorialSequenceBehaviour_2>(graph, owner);
                    break;
                case 3:
                    break;
                case 4:
                    break;
                default:
                    return ScriptPlayable<StartSequenceBehaviour_3>.Create(graph);
            }

            return Playable.Null;
        }

        [SerializeField, Range(1, 5)] private int _tutorialPhase;
    }
}
