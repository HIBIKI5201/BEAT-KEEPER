using System;
using BeatKeeper.Runtime.Ingame.Battle;
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
        private void Awake()
        {
            if (_chart.Length != 32)
                Debug.LogWarning("譜面データの長さが不適切です。");
        }
        
        public float MaxHealth => _maxHealth;
        [SerializeField, Tooltip("最大体力値")] private float _maxHealth = 100;

        public AttackKindEnum[] Chart => _chart;
        [SerializeField, Tooltip("ビートの拍子")] private AttackKindEnum[] _chart = new AttackKindEnum[32];

        public float NockbackTime => _nockbackTime;
        [SerializeField, Tooltip("ノックバック時間")] private float _nockbackTime = 1;

        public bool IsAttack(int index)
        {
            index %= _chart.Length;
            return _chart[index] != AttackKindEnum.None;
        }
    }
}