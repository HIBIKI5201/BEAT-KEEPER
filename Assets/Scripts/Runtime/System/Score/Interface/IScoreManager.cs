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
        public ReadOnlyReactiveProperty<int> ScoreProp { get; }
        public int Score { get; }

        /// <summary>
        /// スコアを増やす（マイナスで減らす）
        /// </summary>
        public void AddScore(int score);

        /// <summary>
        /// バトル開始時にスコアを保存する
        /// </summary>
        public void SavePreBattleScore();
        
        /// <summary>
        /// バトル中に獲得したスコアを計算する
        /// </summary>
        public int CalculateBattleScore();

        /// <summary>
        /// バトルランクを計算する
        /// </summary>
        public void ResetScore();
    }
}
