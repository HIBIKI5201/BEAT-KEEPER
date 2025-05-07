using System;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Bloom = UnityEngine.Rendering.Universal.Bloom;
using ChromaticAberration = UnityEngine.Rendering.Universal.ChromaticAberration;
using ColorParameter = UnityEngine.Rendering.ColorParameter;
using DepthOfField = UnityEngine.Rendering.Universal.DepthOfField;
using LensDistortion = UnityEngine.Rendering.Universal.LensDistortion;
using Vignette = UnityEngine.Rendering.Universal.Vignette;

namespace BeatKeeper
{
    /// <summary>
    /// ポストプロセスエフェクトを制御するためのシングルトンクラス。ゲーム全体で一箇所からエフェクトを制御できるようにする
    /// </summary>
    [DefaultExecutionOrder(1010)]
    public class VolumeController : MonoBehaviour
    {
        [Header("ボリューム設定")]
        [SerializeField] private Volume _globalVolume; // グローバルボリュームへの参照
        private Dictionary<Type, VolumeComponent> _volumeComponents = new Dictionary<Type, VolumeComponent>(); // ボリュームコンポーネントのキャッシュ
        
        [Header("プリセット設定")]
        [SerializeField] private VolumePresetSO[] _presetAssets;
        [SerializeField] private EffectPresetEnum _defaultPreset = EffectPresetEnum.Default;
        private Dictionary<EffectPresetEnum, EffectSettings> _presets = new Dictionary<EffectPresetEnum, EffectSettings>();
        
        // 現在アクティブなTween（キャンセル用）
        private Dictionary<string, Sequence> _activeSequences = new Dictionary<string, Sequence>();
        
        // カメラ
        private Camera _mainCamera;

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

            // 必要なコンポーネントを初期化して辞書に登録
            InitializeVolumeComponent<Vignette>();
            InitializeVolumeComponent<ColorAdjustments>();
            InitializeVolumeComponent<DepthOfField>();
            InitializeVolumeComponent<LensDistortion>();
            InitializeVolumeComponent<Bloom>();
            InitializeVolumeComponent<ChromaticAberration>();
            InitializeVolumeComponent<FilmGrain>();
        }
        
        /// <summary>
        /// 型Tに対応するVolumeコンポーネントを初期化して辞書に登録
        /// </summary>
        private void InitializeVolumeComponent<T>() where T : VolumeComponent
        {
            if (!_globalVolume.profile.TryGet<T>(out var component))
            {
                component = _globalVolume.profile.Add<T>();
            }
            
            _volumeComponents[typeof(T)] = component;
        }
        
        /// <summary>
        /// SerializeFieldに設定されたPresetの配列を元に辞書を作成する
        /// </summary>
        private void CreateVolumePresetDictionary()
        {
            _presets = _presetAssets.ToDictionary(preset => preset.PresetEnumType, preset => preset.Settings);
            ApplyEffectSettings(_presets[_defaultPreset], 0.1f); // 指定したデフォルトプリセットを適用
        }

        private void Start()
        {
             _mainCamera = Camera.main;
             if (_mainCamera == null)
             {
                 Debug.LogWarning("[VolumeController] カメラが見つかりませんでした。");
             }
        }
        
        #endregion
        
        #region エフェクト適用メソッド
        
        /// <summary>
        /// エフェクト設定を適用する
        /// </summary>
        public Sequence ApplyEffectSettings(EffectSettings settings, float duration = 0.5f, Ease easeType = Ease.OutQuad)
        {
            StopAllTweens();
            
            var sequence = DOTween.Sequence();
            _activeSequences["effectSettings"] = sequence;
            
            // エフェクト設定の適用
            // エフェクト設定の適用
            ApplyEffect<Vignette>(settings.EnableVignette, sequence,
                (comp) => ApplyParameter(comp, comp.intensity, settings.VignetteIntensity, sequence, duration, easeType));
                
            ApplyEffect<ColorAdjustments>(settings.EnableColorAdjustments, sequence, (comp) => {
                ApplyParameter(comp, comp.saturation, settings.Saturation, sequence, duration, easeType);
                ApplyParameter(comp, comp.contrast, settings.Contrast, sequence, duration, easeType);
                ApplyParameter(comp, comp.colorFilter, settings.ColorFilter, sequence, duration, easeType);
                return sequence;
            });
            
            ApplyEffect<DepthOfField>(settings.EnableDepthOfField, sequence, (comp) => {
                ApplyParameter(comp, comp.focusDistance, settings.FocusDistance, sequence, duration, easeType);
                ApplyParameter(comp, comp.aperture, settings.Aperture, sequence, duration, easeType);
                ApplyParameter(comp, comp.focalLength, settings.FocalLength, sequence, duration, easeType);
                return sequence;
            });
            
            ApplyEffect<LensDistortion>(settings.EnableLensDistortion, sequence,
                (comp) => ApplyParameter(comp, comp.intensity, settings.LensDistortionIntensity, sequence, duration, easeType));
                
            ApplyEffect<Bloom>(true, sequence, (comp) => {
                ApplyParameter(comp, comp.intensity, settings.BloomIntensity, sequence, duration, easeType);
                ApplyParameter(comp, comp.threshold, settings.BloomThreshold, sequence, duration, easeType);
                return sequence;
            });
            
            ApplyEffect<ChromaticAberration>(settings.EnableChromaticAberration, sequence,
                (comp) => ApplyParameter(comp, comp.intensity, settings.ChromaticAberrationIntensity, sequence, duration, easeType));
                
            ApplyEffect<FilmGrain>(settings.EnableFilmGrain, sequence,
                (comp) => ApplyParameter(comp, comp.intensity, settings.FilmGrainIntensity, sequence, duration, easeType));
            
            // カメラFOV設定
            if (_mainCamera != null && settings.AdjustCameraFov)
            {
                sequence.Join(DOTween.To(() => _mainCamera.fieldOfView, 
                    x => _mainCamera.fieldOfView = x, settings.CameraFov, duration)
                    .SetEase(easeType));
            }
            
            return sequence;
        }
        
        /// <summary>
        /// 個別のパラメータを調整
        /// </summary>
        public Tween AdjustEffect(EffectTypeEnum effectType, float value, float duration = 0.5f, Ease easeType = Ease.OutQuad)
        {
            Tween resultTween = null;
            
            switch (effectType)
            {
                case EffectTypeEnum.VignetteIntensity:
                    resultTween = ApplyEffect<Vignette>(true, null,
                        (comp) => ApplyParameter(comp, comp.intensity, value, null, duration, easeType));
                    break;
                case EffectTypeEnum.Saturation:
                    resultTween = ApplyEffect<ColorAdjustments>(true, null, 
                        (comp) => ApplyParameter(comp, comp.saturation, value, null, duration, easeType));
                    break;
                case EffectTypeEnum.Contrast:
                    resultTween = ApplyEffect<ColorAdjustments>(true, null, 
                        (comp) => ApplyParameter(comp, comp.contrast, value, null, duration, easeType));
                    break;
                case EffectTypeEnum.DepthOfFieldFocus:
                    resultTween = ApplyEffect<DepthOfField>(true, null, 
                        (comp) => ApplyParameter(comp, comp.focusDistance, value, null, duration, easeType));
                    break;
                case EffectTypeEnum.LensDistortion:
                    resultTween = ApplyEffect<LensDistortion>(true, null,
                        (comp) => ApplyParameter(comp, comp.intensity, value, null, duration, easeType));
                    break;
                case EffectTypeEnum.BloomIntensity:
                    resultTween = ApplyEffect<Bloom>(true, null, 
                        (comp) => ApplyParameter(comp, comp.intensity, value, null, duration, easeType));
                    break;
                case EffectTypeEnum.ChromaticAberration:
                    resultTween = ApplyEffect<ChromaticAberration>(true, null, 
                        (comp) => ApplyParameter(comp, comp.intensity, value, null, duration, easeType));
                    break;
                case EffectTypeEnum.FilmGrain:
                    resultTween = ApplyEffect<FilmGrain>(true, null, 
                        (comp) => ApplyParameter(comp, comp.intensity, value, null, duration, easeType));
                    break;
                case EffectTypeEnum.CameraFov:
                    if (_mainCamera != null)
                    {
                        DOTween.To(() => _mainCamera.fieldOfView, x => _mainCamera.fieldOfView = x, value, duration)
                            .SetEase(easeType);
                    }
                    break;
            }

            return resultTween;
        }
        
        /// <summary>
        /// エフェクトコンポーネントへの処理適用
        /// </summary>
        private Tween ApplyEffect<T>(bool enabled, Sequence sequence, Func<T, Tween> applyAction) where T : VolumeComponent
        {
            var component = GetVolumeComponent<T>();
            if (component == null) return null;
            
            component.active = enabled;
            if (!enabled) return null;
            
            return applyAction(component);
        }
        
        /// <summary>
        /// パラメータに値を適用（ClampedFloatParameter用）
        /// </summary>
        private Tween ApplyParameter<T>(T component, ClampedFloatParameter parameter, float targetValue, 
            Sequence sequence, float duration, Ease easeType) where T : VolumeComponent
        {
            var tween = DOTween.To(() => parameter.value, x => parameter.value = x, targetValue, duration).SetEase(easeType);
            sequence?.Join(tween);
            return tween;
        }
        
        /// <summary>
        /// パラメータに値を適用（MinFloatParameter用）
        /// </summary>
        private Tween ApplyParameter<T>(T component, MinFloatParameter parameter, float targetValue, 
            Sequence sequence, float duration, Ease easeType) where T : VolumeComponent 
        {
            var tween = DOTween.To(() => parameter.value, x => parameter.value = x, targetValue, duration).SetEase(easeType);
            sequence?.Join(tween);
            return tween;
        }
        
        /// <summary>
        /// パラメータに値を適用（Color用）
        /// </summary>
        private Tween ApplyParameter<T>(T component, ColorParameter parameter, Color targetValue, 
            Sequence sequence, float duration, Ease easeType) where T : VolumeComponent
        {
            var tween = DOTween.To(() => parameter.value, x => parameter.value = x, targetValue, duration).SetEase(easeType);
            sequence?.Join(tween);
            return tween;
        }
        
        /// <summary>
        /// プリセットを適用
        /// </summary>
        public Sequence ApplyPreset(EffectPresetEnum preset, float duration = 0.5f, Ease easeType = Ease.OutQuad)
        {
            if (_presets.TryGetValue(preset, out var settings))
            {
                return ApplyEffectSettings(settings, duration, easeType);
            }
            else
            {
                Debug.LogWarning($"[VolumeController] プリセット {preset} が見つかりません。");
                return null;
            }
        }

        /// <summary>
        /// カメラを揺らす
        /// </summary>
        public Tween ApplyCameraShake(float duration = 0.2f, float intensity = 0.2f)
        {
            return _mainCamera?.transform?.DOShakePosition(duration, intensity);
        }
        
        #endregion
        
        /// <summary>
        /// 指定した型のVolumeコンポーネントを取得
        /// </summary>
        private T GetVolumeComponent<T>() where T : VolumeComponent
        {
            if (_volumeComponents.TryGetValue(typeof(T), out var component))
            {
                return component as T;
            }
            return null;
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