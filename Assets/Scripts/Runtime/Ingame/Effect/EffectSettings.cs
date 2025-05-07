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
        [Header("ビネット")]
        [SerializeField] private bool _enableVignette = false;
        [SerializeField] private float _vignetteIntensity = 0.25f;
        
        [Header("カラー調整")]
        [SerializeField] private bool _enableColorAdjustments = false;
        [SerializeField] private float _saturation = 0f;
        [SerializeField] private float _contrast = 0f;
        [SerializeField] private Color _colorFilter = Color.white;
        
        [Header("被写界深度")]
        [SerializeField] private bool _enableDepthOfField = false;
        [SerializeField] private float _focusDistance = 10f;
        [SerializeField] private float _aperture = 5.6f;
        [SerializeField] private float _focalLength = 50f;
        
        [Header("レンズ歪み")]
        [SerializeField] private bool _enableLensDistortion = false;
        [SerializeField] private float _lensDistortionIntensity = 0f;
            
        [Header("ブルーム")]
        [SerializeField] private bool _enableBloom = false;
        [SerializeField] private float _bloomIntensity = 1f;
        [SerializeField] private float _bloomThreshold = 0.9f;
        
        [Header("色収差")]
        [SerializeField] private bool _enableChromaticAberration = false;
        [SerializeField] private float _chromaticAberrationIntensity = 0f;
        
        [Header("フィルムグレイン")]
        [SerializeField] private bool _enableFilmGrain = false;
        [SerializeField] private float _filmGrainIntensity = 0f;
        
        [Header("カメラFOV")]
        [SerializeField] private bool _adjustCameraFov = false;
        [SerializeField] private float _cameraFov = 60f;
        
        // プロパティ定義
        public bool EnableVignette => _enableVignette;
        public float VignetteIntensity => _vignetteIntensity;
        
        public bool EnableColorAdjustments => _enableColorAdjustments;
        public float Saturation => _saturation;
        public float Contrast => _contrast;
        public Color ColorFilter => _colorFilter;
        
        public bool EnableDepthOfField => _enableDepthOfField;
        public float FocusDistance => _focusDistance;
        public float Aperture => _aperture;
        public float FocalLength => _focalLength;
        
        public bool EnableLensDistortion => _enableLensDistortion;
        public float LensDistortionIntensity => _lensDistortionIntensity;
        
        public bool EnableBloom => _enableBloom;
        public float BloomIntensity => _bloomIntensity;
        public float BloomThreshold => _bloomThreshold;
        
        public bool EnableChromaticAberration => _enableChromaticAberration;
        public float ChromaticAberrationIntensity => _chromaticAberrationIntensity;
        
        public bool EnableFilmGrain => _enableFilmGrain;
        public float FilmGrainIntensity => _filmGrainIntensity;
        
        public bool AdjustCameraFov => _adjustCameraFov;
        public float CameraFov => _cameraFov;
    }
}
