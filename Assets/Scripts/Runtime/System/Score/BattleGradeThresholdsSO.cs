using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// バトルグレード認定のため各ランクの基準値を設定するためのスクリプタブルオブジェクト
    /// </summary>
    [CreateAssetMenu(fileName = "BattleGradeThresholdsSO", menuName = "BeatKeeper/BattleGradeThresholdsSO")]
    public class BattleGradeThresholdsSO : ScriptableObject
    {
        [SerializeField] private int _thresholdRankSSS = 10000;
        public int ThresholdRankSss => _thresholdRankSSS;
        
        [SerializeField] private int _thresholdRankSS = 8000;
        public int ThresholdRankSs => _thresholdRankSS;
        
        [SerializeField] private int _thresholdRankS = 6000;
        public int ThresholdRankS => _thresholdRankS;
        
        [SerializeField] private int _thresholdRankA = 4000;
        public int ThresholdRankA => _thresholdRankA;
        
        [SerializeField] private int _thresholdRankB = 2000;
        public int ThresholdRankB => _thresholdRankB;
        
        // Cランクも存在するが、Bランク以下は全てCランクとなるので、値は特に設定しない
    }
}
