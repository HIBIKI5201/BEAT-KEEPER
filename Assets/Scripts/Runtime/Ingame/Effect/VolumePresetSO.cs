using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BeatKeeper
{
    /// <summary>
    /// ポストプロセスエフェクト設定用のスクリプタブルオブジェクト
    /// プリセット設定を外部アセットとして保存・管理する
    /// </summary>
    [CreateAssetMenu(fileName = "VolumePresetSO", menuName = "BeatKeeper/VolumePresetSO")]
    public class VolumePresetSO : ScriptableObject
    {
        [FormerlySerializedAs("presetType")] [SerializeField] 
        private EffectPresetEnum presetEnumType = EffectPresetEnum.Custom;
        public EffectPresetEnum PresetEnumType => presetEnumType;

        [SerializeField] 
        private EffectSettings settings = new EffectSettings();
        public EffectSettings Settings => settings;

        // プリセットのコレクションを簡単に作成するための静的メソッド
        public static Dictionary<EffectPresetEnum, EffectSettings> CreateDefaultPresets()
        {
            var presets = new Dictionary<EffectPresetEnum, EffectSettings>();
            
            // デフォルトプリセット
            presets[EffectPresetEnum.Default] = new EffectSettings();
            
            // 戦闘プリセット
            presets[EffectPresetEnum.Combat] = new EffectSettings
            {
                enableVignette = true,
                vignetteIntensity = 0.4f,
                enableBloom = true,
                bloomIntensity = 1.2f,
                enableChromaticAberration = true,
                chromaticAberrationIntensity = 0.2f
            };
            
            // 体力低下プリセット
            presets[EffectPresetEnum.LowHealth] = new EffectSettings
            {
                enableVignette = true,
                vignetteIntensity = 0.7f,
                enableColorAdjustments = true,
                saturation = -30f,
                colorFilter = new Color(1f, 0.5f, 0.5f, 1f),
                enableChromaticAberration = true,
                chromaticAberrationIntensity = 0.5f,
                enableFilmGrain = true,
                filmGrainIntensity = 0.5f
            };
            
            // 勝利プリセット
            presets[EffectPresetEnum.Victory] = new EffectSettings
            {
                enableBloom = true,
                bloomIntensity = 2f,
                enableColorAdjustments = true,
                saturation = 20f,
                contrast = 10f,
                enableDepthOfField = true,
                focusDistance = 3f,
                aperture = 1f
            };
            
            // ゲームオーバープリセット
            presets[EffectPresetEnum.GameOver] = new EffectSettings
            {
                enableVignette = true,
                vignetteIntensity = 0.8f,
                enableColorAdjustments = true,
                saturation = -100f,
                contrast = -20f,
                enableDepthOfField = true,
                focusDistance = 1f,
                enableFilmGrain = true,
                filmGrainIntensity = 0.8f
            };
            
            // スローモーションプリセット
            presets[EffectPresetEnum.SlowMotion] = new EffectSettings
            {
                enableVignette = true,
                vignetteIntensity = 0.6f,
                enableDepthOfField = true,
                focusDistance = 2f,
                enableChromaticAberration = true,
                chromaticAberrationIntensity = 0.3f,
                adjustCameraFov = true,
                cameraFov = 45f
            };
            
            // フラッシュバックプリセット
            presets[EffectPresetEnum.Flashback] = new EffectSettings
            {
                enableVignette = true,
                vignetteIntensity = 0.5f,
                enableColorAdjustments = true,
                saturation = -50f,
                colorFilter = new Color(0.8f, 0.8f, 1f, 1f),
                enableFilmGrain = true,
                filmGrainIntensity = 0.4f
            };
            
            // 夢シーケンスプリセット
            presets[EffectPresetEnum.Dream] = new EffectSettings
            {
                enableBloom = true,
                bloomIntensity = 1.5f,
                enableLensDistortion = true,
                lensDistortionIntensity = 0.2f,
                enableColorAdjustments = true,
                saturation = 10f,
                colorFilter = new Color(0.9f, 0.9f, 1.1f, 1f),
                enableDepthOfField = true,
                focusDistance = 3f
            };
            
            return presets;
        }
    }
}
