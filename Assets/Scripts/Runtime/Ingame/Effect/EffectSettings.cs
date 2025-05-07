using System;
using UnityEngine;

namespace BeatKeeper
{
    /// <summary>
    /// エフェクト設定用のデータクラス
    /// 各エフェクトのパラメータと有効/無効状態を保持
    /// </summary>
    [Serializable]
    public class EffectSettings
    {
        // 各エフェクト用のプロパティ
        public bool enableVignette = false;
        public float vignetteIntensity = 0.25f;
            
        public bool enableColorAdjustments = false;
        public float saturation = 0f;
        public float contrast = 0f;
        public Color colorFilter = Color.white;
            
        public bool enableDepthOfField = false;
        public float focusDistance = 10f;
        public float aperture = 5.6f;
        public float focalLength = 50f;
            
        public bool enableLensDistortion = false;
        public float lensDistortionIntensity = 0f;
            
        public bool enableBloom = false;
        public float bloomIntensity = 1f;
        public float bloomThreshold = 0.9f;
            
        public bool enableChromaticAberration = false;
        public float chromaticAberrationIntensity = 0f;
            
        public bool enableFilmGrain = false;
        public float filmGrainIntensity = 0f;
            
        public bool adjustCameraFov = false;
        public float cameraFov = 60f;
    }
}
