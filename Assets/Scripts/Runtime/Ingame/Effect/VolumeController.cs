using System;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BeatKeeper
{
    /// <summary>
    /// ポストプロセスエフェクトを制御するためのシングルトンクラス
    /// ゲーム全体で一箇所からエフェクトを制御できるようにする
    /// </summary>
    [DefaultExecutionOrder(1010)]
    public class VolumeController : MonoBehaviour
    {
        [Header("ボリューム設定")]
        [SerializeField] private Volume _globalVolume; // グローバルボリュームへの参照
        
        [Header("プリセット設定")]
        [SerializeField] private VolumePresetSO[] _presetAssets;
        [SerializeField] private EffectPresetEnum _defaultPreset = EffectPresetEnum.Default;
        private Dictionary<EffectPresetEnum, EffectSettings> _presets = new Dictionary<EffectPresetEnum, EffectSettings>();
        
        
        // ボリュームコンポーネントのキャッシュ
        private Vignette _vignette;
        private ColorAdjustments _colorAdjustments;
        private DepthOfField _depthOfField;
        private LensDistortion _lensDistortion;
        private Bloom _bloom;
        private ChromaticAberration _chromaticAberration;
        private FilmGrain _filmGrain;
        
        // デフォルト値
        private float _defaultVignetteIntensity;
        private float _defaultSaturation;
        private float _defaultContrast;
        private float _defaultBloomIntensity;
        private float _defaultChromaticAberration;
        private float _defaultLensDistortion;
        private float _defaultFilmGrainIntensity;
        private float _defaultDepthOfFieldFocusDistance;
        
        // 現在アクティブなTween（キャンセル用）
        private Dictionary<string, Sequence> _activeSequences = new Dictionary<string, Sequence>();
        
        // カメラ
        private Camera _mainCamera;
        private float _defaultFov;

        #region セットアップ・初期化処理

        private void Awake()
        {
            ServiceLocator.SetInstance(this, ServiceLocator.LocateType.Singleton); // シングルトンに登録
            Initialize();
        }
        
        /// <summary>
        /// 初期化処理のメインフロー
        /// </summary>
        private void Initialize()
        {
            InitializeGlobalVolume();
            InitializeVolumeComponents();
            CreateVolumePresetDictionary();
        }

        /// <summary>
        /// グローバルボリュームの設定
        /// </summary>
        private void InitializeGlobalVolume()
        {
            if (_globalVolume != null) return;

            _globalVolume = FindFirstObjectByType<Volume>();

            if (_globalVolume == null)
            {
                // シーン内にVolumeコンポーネントがなかったら、自身のオブジェクトに新しいボリュームを追加
                Debug.LogWarning("[VolumeController] GlobalVolumeコンポーネントが見つかりません。新規作成します");
                _globalVolume = gameObject.AddComponent<Volume>();
                _globalVolume.isGlobal = true;
            }
        }

        /// <summary>
        /// Volumeコンポーネントの参照を取得する
        /// </summary>
        private void InitializeVolumeComponents()
        {
            if (_globalVolume.profile == null) return;

            InitializeVolumeComponent(ref _vignette, c =>
                _defaultVignetteIntensity = c.intensity.value);

            InitializeVolumeComponent(ref _colorAdjustments, c =>
            {
                _defaultSaturation = c.saturation.value;
                _defaultContrast = c.contrast.value;
            });

            InitializeVolumeComponent(ref _depthOfField, c =>
                _defaultDepthOfFieldFocusDistance = c.focusDistance.value);

            InitializeVolumeComponent(ref _lensDistortion, c =>
                _defaultLensDistortion = c.intensity.value);

            InitializeVolumeComponent(ref _bloom, c =>
                _defaultBloomIntensity = c.intensity.value);

            InitializeVolumeComponent(ref _chromaticAberration, c =>
                _defaultChromaticAberration = c.intensity.value);

            InitializeVolumeComponent(ref _filmGrain, c =>
                _defaultFilmGrainIntensity = c.intensity.value);
        }
        
        /// <summary>
        /// Volumeコンポーネントの初期化のためのメソッド
        /// </summary>
        private void InitializeVolumeComponent<T>(ref T component, Action<T> setUpAction = null) where T : VolumeComponent
        {
            if (!_globalVolume.profile.TryGet(out component))
            {
                component = _globalVolume.profile.Add<T>();
            }
            setUpAction?.Invoke(component);
        }
        
        /// <summary>
        /// SerializeFieldに設定されたPresetの配列を元に辞書を作成する
        /// </summary>
        private void CreateVolumePresetDictionary()
        {
            _presets.Clear();
            foreach (var preset in _presetAssets)
            {
                _presets.Add(preset.PresetEnumType, preset.Settings);
            }
            
            ApplyEffectSettings(_presets[_defaultPreset], 0.1f); // 指定したデフォルトプリセットを適用
        }

        private void Start()
        {
            // メインカメラの取得（複数シーンを読み込んだ時別シーンにカメラがあるので、この方法をとっている）
            var allCameras = Camera.allCameras;
    
            _mainCamera = allCameras.FirstOrDefault(camera => camera.CompareTag("MainCamera"));
    
            if (_mainCamera == null && allCameras.Length > 0)
            {
                Debug.LogWarning("[VolumeController] MainCameraタグのカメラが見つかりませんでした。最初に見つかったカメラを使用します。");
                _mainCamera = allCameras[0];
            }
        }
        
        #endregion
        
        #region 効果適用メソッド
        
        /// <summary>
        /// エフェクト設定を適用する主要メソッド
        /// </summary>
        /// <param name="settings">適用するエフェクト設定</param>
        /// <param name="duration">トランジション時間（秒）</param>
        /// <param name="easeType">イージングタイプ</param>
        public void ApplyEffectSettings(EffectSettings settings, float duration = 0.5f, Ease easeType = Ease.OutQuad)
        {
            // すべてのアクティブなシーケンスを停止
            StopAllTweens();
            
            // 新しいシーケンスを作成
            Sequence effectSequence = DOTween.Sequence();
            _activeSequences["effectSettings"] = effectSequence;
            
            // ビネット
            if (_vignette != null)
            {
                _vignette.active = settings.EnableVignette;
                if (settings.EnableVignette)
                {
                    effectSequence.Join(DOTween.To(() => _vignette.intensity.value, 
                        x => _vignette.intensity.value = x, settings.VignetteIntensity, duration)
                        .SetEase(easeType));
                }
            }
            
            // 色調整
            if (_colorAdjustments != null)
            {
                _colorAdjustments.active = settings.EnableColorAdjustments;
                if (settings.EnableColorAdjustments)
                {
                    effectSequence.Join(DOTween.To(() => _colorAdjustments.saturation.value, 
                        x => _colorAdjustments.saturation.value = x, settings.Saturation, duration)
                        .SetEase(easeType));
                    
                    effectSequence.Join(DOTween.To(() => _colorAdjustments.contrast.value, 
                        x => _colorAdjustments.contrast.value = x, settings.Contrast, duration)
                        .SetEase(easeType));
                    
                    effectSequence.Join(DOTween.To(() => _colorAdjustments.colorFilter.value, 
                        x => _colorAdjustments.colorFilter.value = x, settings.ColorFilter, duration)
                        .SetEase(easeType));
                }
            }
            
            // 被写界深度
            if (_depthOfField != null)
            {
                _depthOfField.active = settings.EnableDepthOfField;
                if (settings.EnableDepthOfField)
                {
                    effectSequence.Join(DOTween.To(() => _depthOfField.focusDistance.value, 
                        x => _depthOfField.focusDistance.value = x, settings.FocusDistance, duration)
                        .SetEase(easeType));
                    
                    effectSequence.Join(DOTween.To(() => _depthOfField.aperture.value, 
                        x => _depthOfField.aperture.value = x, settings.Aperture, duration)
                        .SetEase(easeType));
                    
                    effectSequence.Join(DOTween.To(() => _depthOfField.focalLength.value, 
                        x => _depthOfField.focalLength.value = x, settings.FocalLength, duration)
                        .SetEase(easeType));
                }
            }
            
            // レンズディストーション
            if (_lensDistortion != null)
            {
                _lensDistortion.active = settings.EnableLensDistortion;
                if (settings.EnableLensDistortion)
                {
                    effectSequence.Join(DOTween.To(() => _lensDistortion.intensity.value, 
                        x => _lensDistortion.intensity.value = x, settings.LensDistortionIntensity, duration)
                        .SetEase(easeType));
                }
            }
            
            // ブルーム
            if (_bloom != null)
            {
                _bloom.active = settings.EnableBloom;
                if (settings.EnableBloom)
                {
                    effectSequence.Join(DOTween.To(() => _bloom.intensity.value, 
                        x => _bloom.intensity.value = x, settings.BloomIntensity, duration)
                        .SetEase(easeType));
                    
                    effectSequence.Join(DOTween.To(() => _bloom.threshold.value, 
                        x => _bloom.threshold.value = x, settings.BloomThreshold, duration)
                        .SetEase(easeType));
                }
            }
            
            // 色収差
            if (_chromaticAberration != null)
            {
                _chromaticAberration.active = settings.EnableChromaticAberration;
                if (settings.EnableChromaticAberration)
                {
                    effectSequence.Join(DOTween.To(() => _chromaticAberration.intensity.value, 
                        x => _chromaticAberration.intensity.value = x, settings.ChromaticAberrationIntensity, duration)
                        .SetEase(easeType));
                }
            }
            
            // フィルムグレイン
            if (_filmGrain != null)
            {
                _filmGrain.active = settings.EnableFilmGrain;
                if (settings.EnableFilmGrain)
                {
                    effectSequence.Join(DOTween.To(() => _filmGrain.intensity.value, 
                        x => _filmGrain.intensity.value = x, settings.FilmGrainIntensity, duration)
                        .SetEase(easeType));
                }
            }
            
            // カメラFOV
            if (_mainCamera != null && settings.AdjustCameraFov)
            {
                effectSequence.Join(DOTween.To(() => _mainCamera.fieldOfView, 
                    x => _mainCamera.fieldOfView = x, settings.CameraFov, duration)
                    .SetEase(easeType));
            }
        }
        
        /// <summary>
        /// 個別のパラメータだけを調整できるクイックメソッド
        /// </summary>
        /// <param name="effectTypeEnum">調整するエフェクトタイプ</param>
        /// <param name="value">設定する値</param>
        /// <param name="duration">トランジション時間（秒）</param>
        /// <param name="easeType">イージングタイプ</param>
        public void AdjustEffect(EffectTypeEnum effectTypeEnum, float value, float duration = 0.5f, Ease easeType = Ease.OutQuad)
        {
            // エフェクトタイプに応じて異なるパラメータを調整
            switch (effectTypeEnum)
            {
                case EffectTypeEnum.VignetteIntensity:
                    if (_vignette != null)
                    {
                        _vignette.active = true;
                        DOTween.To(() => _vignette.intensity.value, 
                            x => _vignette.intensity.value = x, value, duration).SetEase(easeType);
                    }
                    break;
                
                case EffectTypeEnum.Saturation:
                    if (_colorAdjustments != null)
                    {
                        _colorAdjustments.active = true;
                        DOTween.To(() => _colorAdjustments.saturation.value, 
                            x => _colorAdjustments.saturation.value = x, value, duration).SetEase(easeType);
                    }
                    break;
                
                case EffectTypeEnum.Contrast:
                    if (_colorAdjustments != null)
                    {
                        _colorAdjustments.active = true;
                        DOTween.To(() => _colorAdjustments.contrast.value, 
                            x => _colorAdjustments.contrast.value = x, value, duration).SetEase(easeType);
                    }
                    break;
                
                case EffectTypeEnum.DepthOfFieldFocus:
                    if (_depthOfField != null)
                    {
                        _depthOfField.active = true;
                        DOTween.To(() => _depthOfField.focusDistance.value, 
                            x => _depthOfField.focusDistance.value = x, value, duration).SetEase(easeType);
                    }
                    break;
                
                case EffectTypeEnum.LensDistortion:
                    if (_lensDistortion != null)
                    {
                        _lensDistortion.active = true;
                        DOTween.To(() => _lensDistortion.intensity.value, 
                            x => _lensDistortion.intensity.value = x, value, duration).SetEase(easeType);
                    }
                    break;
                
                case EffectTypeEnum.BloomIntensity:
                    if (_bloom != null)
                    {
                        _bloom.active = true;
                        DOTween.To(() => _bloom.intensity.value, 
                            x => _bloom.intensity.value = x, value, duration).SetEase(easeType);
                    }
                    break;
                
                case EffectTypeEnum.ChromaticAberration:
                    if (_chromaticAberration != null)
                    {
                        _chromaticAberration.active = true;
                        DOTween.To(() => _chromaticAberration.intensity.value, 
                            x => _chromaticAberration.intensity.value = x, value, duration).SetEase(easeType);
                    }
                    break;
                
                case EffectTypeEnum.FilmGrain:
                    if (_filmGrain != null)
                    {
                        _filmGrain.active = true;
                        DOTween.To(() => _filmGrain.intensity.value, 
                            x => _filmGrain.intensity.value = x, value, duration).SetEase(easeType);
                    }
                    break;
                
                case EffectTypeEnum.CameraFov:
                    if (_mainCamera != null)
                    {
                        DOTween.To(() => _mainCamera.fieldOfView, 
                            x => _mainCamera.fieldOfView = x, value, duration).SetEase(easeType);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 色調整用のカラーフィルターを設定するためのメソッド
        /// </summary>
        /// <param name="color">設定する色</param>
        /// <param name="duration">トランジション時間（秒）</param>
        /// <param name="easeType">イージングタイプ</param>
        public void SetColorFilter(Color color, float duration = 0.5f, Ease easeType = Ease.OutQuad)
        {
            if (_colorAdjustments != null)
            {
                _colorAdjustments.active = true;
                DOTween.To(() => _colorAdjustments.colorFilter.value, 
                    x => _colorAdjustments.colorFilter.value = x, color, duration).SetEase(easeType);
            }
        }
        
        
        /// <summary>
        /// プリセットを使用してエフェクトを適用
        /// </summary>
        /// <param name="preset">使用するプリセット</param>
        /// <param name="duration">トランジション時間（秒）</param>
        /// <param name="easeType">イージングタイプ</param>
        public void ApplyPreset(EffectPresetEnum preset, float duration = 0.5f, Ease easeType = Ease.OutQuad)
        {
            if (_presets.TryGetValue(preset, out var settings))
            {
                ApplyEffectSettings(settings, duration, easeType);
            }
        }

        /// <summary>
        /// カメラを揺らす
        /// </summary>
        public void CameraShake(float duration = 0.2f, float intensity = 0.2f)
        {
            if (_mainCamera != null && _mainCamera.transform != null)
            {
                _mainCamera.transform.DOShakePosition(duration, intensity);
            }
        }
        
        #endregion
        
        /// <summary>
        /// リセット - すべてのエフェクトをデフォルト値に戻す
        /// </summary>
        public void ResetAllEffects(float duration = 0.5f)
        {
            // すべてのアクティブなシーケンスを停止
            StopAllTweens();
            
            // 新しいシーケンスを作成
            Sequence resetSequence = DOTween.Sequence();
            _activeSequences["reset"] = resetSequence;
            
            // ビネット
            if (_vignette != null)
            {
                resetSequence.Join(DOTween.To(() => _vignette.intensity.value, 
                    x => _vignette.intensity.value = x, _defaultVignetteIntensity, duration));
            }
            
            // 色調整
            if (_colorAdjustments != null)
            {
                resetSequence.Join(DOTween.To(() => _colorAdjustments.saturation.value, 
                    x => _colorAdjustments.saturation.value = x, _defaultSaturation, duration));
                resetSequence.Join(DOTween.To(() => _colorAdjustments.contrast.value, 
                    x => _colorAdjustments.contrast.value = x, _defaultContrast, duration));
            }
            
            // 被写界深度
            if (_depthOfField != null)
            {
                resetSequence.Join(DOTween.To(() => _depthOfField.focusDistance.value, 
                    x => _depthOfField.focusDistance.value = x, 10f, duration));
                resetSequence.OnComplete(() => _depthOfField.active = false);
            }
            
            // レンズディストーション
            if (_lensDistortion != null)
            {
                resetSequence.Join(DOTween.To(() => _lensDistortion.intensity.value, 
                    x => _lensDistortion.intensity.value = x, 0f, duration));
                resetSequence.OnComplete(() => _lensDistortion.active = false);
            }
            
            // ブルーム
            if (_bloom != null)
            {
                resetSequence.Join(DOTween.To(() => _bloom.intensity.value, 
                    x => _bloom.intensity.value = x, _defaultBloomIntensity, duration));
            }
            
            // 色収差
            if (_chromaticAberration != null)
            {
                resetSequence.Join(DOTween.To(() => _chromaticAberration.intensity.value, 
                    x => _chromaticAberration.intensity.value = x, 0f, duration));
                resetSequence.OnComplete(() => _chromaticAberration.active = false);
            }
            
            // フィルムグレイン
            if (_filmGrain != null)
            {
                resetSequence.OnComplete(() => _filmGrain.active = false);
            }
            
            // カメラFOV
            if (_mainCamera != null)
            {
                resetSequence.Join(DOTween.To(() => _mainCamera.fieldOfView, 
                    x => _mainCamera.fieldOfView = x, _defaultFov, duration));
            }
        }
        
        /// <summary>
        /// 特定の名前のTweenを停止
        /// </summary>
        private void StopTween(string tweenName)
        {
            if (_activeSequences.TryGetValue(tweenName, out Sequence sequence))
            {
                sequence.Kill();
                _activeSequences.Remove(tweenName);
            }
        }
        
        /// <summary>
        /// すべてのTweenを停止
        /// </summary>
        private void StopAllTweens()
        {
            foreach (var sequence in _activeSequences.Values)
            {
                sequence.Kill();
            }
            _activeSequences.Clear();
        }
        
        private void OnDestroy()
        {
            StopAllTweens(); // すべてのTweenを停止
            ServiceLocator.DestroyInstance(this);
        }
    }
}