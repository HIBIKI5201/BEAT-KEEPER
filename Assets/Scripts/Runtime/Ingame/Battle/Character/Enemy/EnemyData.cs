using System;
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

        public bool[] Chart => _chart;
        [SerializeField, Tooltip("ビートの拍子")] private bool[] _chart = new bool[32];

        public float NockbackTime => _nockbackTime;
        [SerializeField, Tooltip("ノックバック時間")] private float _nockbackTime = 1;
    }
}