using R3;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// スコアを管理するシステム
    /// </summary>
    public class ScoreManager : MonoBehaviour, IScoreManager
    {
        public ReadOnlyReactiveProperty<int> Score => _score;
        private readonly ReactiveProperty<int> _score = new ReactiveProperty<int>();

        private int _preBattleScore; // バトルが始まる直前のスコア（バトルリザルトの判定用）

        /// <summary>
        /// スコアを増やす（マイナスで減らす）
        /// </summary>
        public void AddScore(int score)
        {
            // スコアはゼロ以下にはならない
            _score.Value = Mathf.Max(_score.Value + score, 0);
        }

        /// <summary>
        /// スコアをリセットする
        /// </summary>
        private void ResetScore()
        {
            _score.Value = 0;
        }
    }
}
