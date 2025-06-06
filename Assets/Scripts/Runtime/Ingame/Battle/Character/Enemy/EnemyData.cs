using System;
using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     エネミーのデータクラス
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = CharacterData.CHARACTER_DATA_DIRECTORY + "EnemyData")]
    public class EnemyData : CharacterData
    {
        public float MaxHealth => _maxHealth;
        [SerializeField, Tooltip("最大体力値")] private float _maxHealth = 100;

        [SerializeField, Tooltip("フィニッシャーの閾値"), Range(0, 100)] private float _finisherThreshold = 10;
        public float FinisherThreshold => _finisherThreshold;

        [SerializeField, Tooltip("ノックバック時間")] private float _nockbackTime = 1;
        public float NockbackTime => _nockbackTime;

        [SerializeField, Tooltip("譜面データ")] private ChartData _chartData;
        public ChartData ChartData => _chartData;
    }
}