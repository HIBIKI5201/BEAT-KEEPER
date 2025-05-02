using System;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// コンボの閾値と倍率の設定
    /// </summary>
    [Serializable]
    public class ComboThreshold
    {
        [SerializeField, Tooltip("この数以上のコンボで適用")] private int _comboCount;
        public int ComboCount => _comboCount;
            
        [SerializeField, Tooltip("スコア倍率")] private float _multiplier;
        public float Multiplier => _multiplier;

        public ComboThreshold(int comboCount, float multiplier)
        {
            _comboCount = comboCount;
            _multiplier = multiplier;
        }
    }
}
