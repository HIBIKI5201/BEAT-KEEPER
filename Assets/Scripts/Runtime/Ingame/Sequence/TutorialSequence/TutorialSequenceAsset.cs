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
            switch (_tutorialFase)
            {
                case 1:
                //return SequenceBehaviourBase.CreatePlayable<TutorialSequenceBehaviour_1>(graph, owner);
                case 2:
                //return SequenceBehaviourBase.CreatePlayable<TutorialSequenceBehaviour_2>(graph, owner);
                case 3:
                //return SequenceBehaviourBase.CreatePlayable<TutorialSequenceBehaviour_3>(graph, owner);
                case 4:
                    break;
                //return SequenceBehaviourBase.CreatePlayable<TutorialSequenceBehaviour_4>(graph, owner);
                case 5:
                    return ScriptPlayable<StartSequenceBehaviour_3>.Create(graph);
            }
            return SequenceBehaviourBase.CreatePlayable<TutorialSequenceBehaviour_1>(graph, owner);
        }

        [SerializeField, Range(1, 5)] private int _tutorialFase;
    }
}
