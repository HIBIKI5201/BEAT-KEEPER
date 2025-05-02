using System;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// コンボ数によるスコア倍率を設定するためのスクリプタブルオブジェクト
    /// </summary>
    [CreateAssetMenu(fileName = "ComboBonusSettingsSO", menuName = "BeatKeeper/ComboBonusSettingsSO")]
    public class ComboBonusSettingsSO : ScriptableObject
    {
        [Serializable]
        public class ComboThreshold
        {
            public int ComboCount;      // この数以上のコンボで適用
            public float Multiplier;    // スコア倍率
        }
        
        [Header("コンボ数の低い順に設定してください")]
        public ComboThreshold[] thresholds = {
            new ComboThreshold { ComboCount = 10, Multiplier = 1.2f },
            new ComboThreshold { ComboCount = 30, Multiplier = 1.5f },
            new ComboThreshold { ComboCount = 50, Multiplier = 2.0f },
            new ComboThreshold { ComboCount = 100, Multiplier = 3.0f }
        };
        
        /// <summary>
        /// 指定されたコンボ数に基づいてボーナス倍率を返す
        /// </summary>
        public float GetBonusMultiplier(int comboCount)
        {
            float multiplier = 1.0f; // デフォルト倍率
            
            // 適用可能な最高の倍率を探す（高いコンボ数から検索）
            for (int i = thresholds.Length - 1; i >= 0; i--)
            {
                if (comboCount >= thresholds[i].ComboCount)
                {
                    multiplier = thresholds[i].Multiplier;
                    break;
                }
            }
            
            return multiplier;
        }
    }
}
