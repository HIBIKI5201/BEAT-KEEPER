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
                    return SequenceBehaviourBase.CreatePlayable<TutorialSequenceBehaviour_1>(graph, owner);
                case 2:
                    return SequenceBehaviourBase.CreatePlayable<TutorialSequenceBehaviour_2>(graph, owner);
                case 3:
                    return SequenceBehaviourBase.CreatePlayable<TutorialSequenceBehaviour_3>(graph, owner);
                case 4:
                    return SequenceBehaviourBase.CreatePlayable<TutorialSequenceBehaviour_4>(graph, owner);
                case 5:
                    return SequenceBehaviourBase.CreatePlayable<TutorialSequenceBehaviour_5>(graph, owner);
            }
        }

        [SerializeField, Range(1, 3)] private int _tutorialFase;
    }
}
