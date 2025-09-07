using UnityEngine;
using UnityEngine.Playables;

namespace BeatKeeper.Runtime.Ingame.Sequence
{
    /// <summary>
    ///     SequenceBehaviourのベースクラス
    /// </summary>
    public abstract class SequenceBehaviourBase : PlayableBehaviour
    {
        public void OnCreate(GameObject owner) => _owner = owner;

        /// <summary>
        ///   Playableを生成してオーナーを設定するヘルパーメソッド
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="graph"></param>
        /// <param name="owner"></param>
        /// <returns></returns>
        public static ScriptPlayable<T> CreatePlayable<T>(PlayableGraph graph, GameObject owner)
            where T : SequenceBehaviourBase, IPlayableBehaviour, new()
        {
            var playable = ScriptPlayable<T>.Create(graph);
            var behaviour = playable.GetBehaviour();
            behaviour.OnCreate(owner);
            return playable;
        }

        protected GameObject _owner;
    }
}
