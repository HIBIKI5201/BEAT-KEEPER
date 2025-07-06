using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    /// <summary>
    /// バトルグレード認定のため各ランクの基準値を設定するためのスクリプタブルオブジェクト
    /// </summary>
    [CreateAssetMenu(fileName = "BattleGradeThresholdsSO", menuName = "BeatKeeper/BattleGradeThresholdsSO")]
    public class BattleGradeThresholdsSO : ScriptableObject
    {
        public int ThresholdRankSss => _thresholdRankSSS;
        public int ThresholdRankSs => _thresholdRankSS;
        public int ThresholdRankS => _thresholdRankS;
        public int ThresholdRankA => _thresholdRankA;
        public int ThresholdRankB => _thresholdRankB;

        [SerializeField] private int _thresholdRankSSS = 10000;
        [SerializeField] private int _thresholdRankSS = 8000;
        [SerializeField] private int _thresholdRankS = 6000;
        [SerializeField] private int _thresholdRankA = 4000;
        [SerializeField] private int _thresholdRankB = 2000;
    }
}
