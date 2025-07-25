﻿using BeatKeeper.Runtime.Ingame.Battle;
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
        /// 今回のバトルで獲得したスコアを取得する
        /// </summary>
        public int CalculateBattleScore()
        {
            return _scoreManager.CalculateBattleScore();
        }

        /// <summary>
        /// バトルランクを計算する
        /// TODO: クリアフェーズで使用する
        /// </summary>
        public BattleGradeEnum EvaluateRank()
        {
            int battleScore = _scoreManager.CalculateBattleScore();
            Debug.Log($"[BattleGradeEvaluator] バトルで獲得したスコア{battleScore}");

            return battleScore switch
            {
                var score when score > _threshold.ThresholdRankSss => BattleGradeEnum.SSS,
                var score when score > _threshold.ThresholdRankSs => BattleGradeEnum.SS,
                var score when score > _threshold.ThresholdRankS => BattleGradeEnum.S,
                var score when score > _threshold.ThresholdRankA => BattleGradeEnum.A,
                var score when score > _threshold.ThresholdRankB => BattleGradeEnum.B,
                _ => BattleGradeEnum.C
            };
        }

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
    }
}
