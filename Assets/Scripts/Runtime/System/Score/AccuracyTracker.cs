using UnityEngine;
using System;
using BeatKeeper.Runtime.Ingame.Character;

namespace BeatKeeper
{
    /// <summary>
    /// Perfect/Good/Missなどの精度記録用のクラス
    /// </summary>
    public class AccuracyTracker
    {
        private PlayerManager _playerManager; // 各アクション購読用のPlayerManagerの参照
        private int _perfectCount = 0; // Perfect判定の回数
        private int _goodCount = 0; // Good判定の回数
        private int _missCount = 0; // Miss判定の回数
        
        /// <summary>
        /// Perfect判定の回数
        /// </summary>
        public int PerfectCount => _perfectCount;
        
        /// <summary>
        /// Good判定の回数
        /// </summary>
        public int GoodCount => _goodCount;

        /// <summary>
        /// Miss判定の回数
        /// </summary>
        public int MissCount => _missCount;
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AccuracyTracker(PlayerManager playerManager)
        {
            _playerManager = playerManager;
            Subscribe();
        }
        
        /// <summary>
        /// PlayerManagerの各アクションの購読を行う
        /// </summary>
        private void Subscribe()
        {
            // 通常攻撃
            _playerManager.OnPerfectAttack += RecordPerfect;
            _playerManager.OnGoodAttack += RecordGood;
            // TODO: 通常攻撃のミス判定
            
            // スキル
            _playerManager.OnPerfectSkill += RecordPerfect;
            _playerManager.OnGoodSkill += RecordGood;
            // TODO: スキルのミス判定
            
            // 回避
            _playerManager.OnPerfectAvoid += RecordPerfect;
            _playerManager.OnGoodAvoid += RecordGood;
            _playerManager.OnFailedAvoid += RecordMiss;
            
            // ため攻撃
            // TODO: ため攻撃のイベント登録
        }

        /// <summary>
        /// Perfectの回数を記録
        /// </summary>
        private void RecordPerfect()
        {
            _perfectCount++;
        }

        /// <summary>
        /// Goodの回数を記録
        /// </summary>
        private void RecordGood()
        {
            _goodCount++;
        }

        /// <summary>
        /// Missの回数を記録
        /// </summary>
        private void RecordMiss()
        {
            _missCount++;
        }
    }
}
