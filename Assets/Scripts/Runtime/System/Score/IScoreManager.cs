using R3;

namespace BeatKeeper
{
    /// <summary>
    /// ScoreManagerのインターフェース
    /// </summary>
    public interface IScoreManager
    {
        /// <summary>
        /// スコア
        /// </summary>
        public ReadOnlyReactiveProperty<int> Score { get; }

        /// <summary>
        /// スコアを増やす（マイナスで減らす）
        /// </summary>
        public void AddScore(int score);
    }
}
