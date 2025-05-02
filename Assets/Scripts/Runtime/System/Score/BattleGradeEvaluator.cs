using UnityEngine;
using UnityEngine.Serialization;

namespace BeatKeeper
{
    /// <summary>
    /// バトルランクを判定する処理を行うクラス
    /// </summary>
    [RequireComponent(typeof(IScoreManager))]
    public class BattleGradeEvaluator : MonoBehaviour
    {
        [SerializeField] private BattleGradeThresholdsSO _threshold;
        private IScoreManager _scoreManager;

        private void Awake()
        {
            _scoreManager = GetComponent<IScoreManager>();
            if (_threshold == null)
            {
                Debug.LogError("[BattleGradeEvaluator] calculateRankDataSO が設定されていません！");
            }
        }

        /// <summary>
        /// バトルランクを計算する
        /// TODO: クリアフェーズで使用する
        /// </summary>
        public BattleGradeEnum EvaluateRank()
        {
            int battleScore = _scoreManager.CalculateBattleScore();

            if (battleScore > _threshold.ThresholdRankSss) return BattleGradeEnum.SSS;
            if (battleScore > _threshold.ThresholdRankSs) return BattleGradeEnum.SS;
            if (battleScore > _threshold.ThresholdRankS) return BattleGradeEnum.S;
            if (battleScore > _threshold.ThresholdRankA) return BattleGradeEnum.A;
            if (battleScore > _threshold.ThresholdRankB) return BattleGradeEnum.B;
            return BattleGradeEnum.C;
        }
    }
}
