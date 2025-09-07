using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.System
{
    /// <summary>
    /// コンボの閾値と倍率の設定
    /// </summary>
    [Serializable]
    public class ComboThreshold
    {
        public ComboThreshold(int comboCount, float multiplier)
        {
            _comboCount = comboCount;
            _multiplier = multiplier;
        }

        public float Multiplier => _multiplier;
        public int ComboCount => _comboCount;

        [SerializeField, Tooltip("この数以上のコンボで適用")] private int _comboCount;
        [SerializeField, Tooltip("スコア倍率")] private float _multiplier;
    }
}
