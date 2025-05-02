using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// コンボ数によるスコア倍率を設定するためのスクリプタブルオブジェクト
    /// </summary>
    [CreateAssetMenu(fileName = "ComboBonusSettingsSO", menuName = "BeatKeeper/ComboBonusSettingsSO")]
    public class ComboBonusSettingsSO : ScriptableObject
    {
        [Header("コンボ数の低い順に設定してください")]
        [SerializeField] private ComboThreshold[] _thresholds = {
            new ComboThreshold(10, 1.2f),
            new ComboThreshold(30, 1.5f),
            new ComboThreshold(50, 2.0f),
            new ComboThreshold(100, 3.0f),
        };
        public ComboThreshold[] Thresholds => _thresholds;
        
        /// <summary>
        /// 指定されたコンボ数に基づいてボーナス倍率を返す
        /// </summary>
        public float GetBonusMultiplier(int comboCount)
        {
            float multiplier = 1.0f; // デフォルト倍率
            
            // 適用可能な最高の倍率を探す（高いコンボ数から検索）
            for (int i = _thresholds.Length - 1; i >= 0; i--)
            {
                if (comboCount >= _thresholds[i].ComboCount)
                {
                    return _thresholds[i].Multiplier;
                }
            }
            
            return 1.0f;
        }
    }
}
