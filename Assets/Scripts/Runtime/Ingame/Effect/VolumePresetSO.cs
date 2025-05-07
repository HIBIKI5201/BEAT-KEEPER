using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// ポストプロセスエフェクトのプリセットを設定するためのスクリプタブルオブジェクト
    /// </summary>
    [CreateAssetMenu(fileName = "VolumePresetSO", menuName = "BeatKeeper/VolumePresetSO")]
    public class VolumePresetSO : ScriptableObject
    {
        // プリセットの種類
        [SerializeField] private EffectPresetEnum _presetEnumType = EffectPresetEnum.Default;
        public EffectPresetEnum PresetEnumType => _presetEnumType;

        // プリセットの設定
        [SerializeField] private EffectSettings _settings = new EffectSettings();
        public EffectSettings Settings => _settings;
    }
}
