using BeatKeeper.Runtime.Ingame.Battle;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    /// <summary>
    /// バトルランクを判定する処理を行うクラス
    /// </summary>
    [RequireComponent(typeof(IScoreManager))]
    public class BattleGradeEvaluator : MonoBehaviour
    {
        /// <summary>
        /// 引数に渡したスコアを元にバトルランクを計算する
        /// </summary>
        public BattleGradeEnum EvaluateRank(int battleScore)
        {
            return battleScore switch
            {
                var score when score > _threshold.ThresholdRankS => BattleGradeEnum.S,
                var score when score > _threshold.ThresholdRankA => BattleGradeEnum.A,
                var score when score > _threshold.ThresholdRankB => BattleGradeEnum.B,
                _ => BattleGradeEnum.C
            };
        }

        [SerializeField] private BattleGradeThresholdsSO _threshold;

        private void Awake()
        {
            if (_threshold == null)
            {
                Debug.LogError("[BattleGradeEvaluator] calculateRankDataSO が設定されていません！");
            }
        }
    }
}
