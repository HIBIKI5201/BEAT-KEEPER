using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BeatKeeper
{
    /// <summary>
    /// ビートクリスタルのアニメーションを管理するクラス
    /// </summary>
    public class BeatCrystalController : MonoBehaviour
    {
        [Header("アニメーションの設定")]
        [SerializeField] private float _defaultRotateDuration = 5f; // 回転アニメーションのデフォルトの速度
        [SerializeField] private float _lockOnRotationDuration = 3f; // ロックオン中の速度
        [SerializeField] private Color _emissionColor = Color.cyan;
        [SerializeField] private Color _lockOnColor = Color.green;
        [SerializeField] private float _emissionIntensity = 5f;
        [SerializeField] private float _pulseSpeed = 2f; // 明滅の速度
        [SerializeField] private float _pulseMagnitude = 0.2f; // 明滅の強さ（最大値からの変動幅）
        
        [Header("コンポーネント")]
        [SerializeField] private ParticleSystem _ambientLightParticle; // 周囲に漂う小さなパーティクル
        [SerializeField] private ParticleSystem _collectionParticle; // 回収時の爆発エフェクト
        [SerializeField] private ParticleSystem _playerAbsorbParticle; // プレイヤーに吸収されるエフェクト
       
        [Header("画面効果")]
        [SerializeField] private Volume _postProcessingVolume; // URPのポストプロセス用Volume
        [SerializeField] private float _lockOnFovChange = 10f; // ロックオン時のFOV変化量
        [SerializeField] private float _collectionFovPulse = 15f; // 回収時のFOV変化量
        [SerializeField] private float _cameraShakeIntensity = 0.2f; // カメラ振動の強さ
        [SerializeField] private float _cameraShakeDuration = 0.2f; // カメラ振動の時間
        
        
        // ポストプロセス用プロファイル要素
        private Vignette _vignette;
        private ColorAdjustments _colorAdjustments;
        private DepthOfField _depthOfField;
        private LensDistortion _lensDistortion;
        
        private Renderer _rend;
        private Tweener _rotateTweener;
        private Tweener _pulseEmissionTweener;
        private Sequence _lockOnEffectSequence;
        
        // カメラ
        private Camera _mainCamera;
        private float _defaultFov;
        
        private bool _isLockedOn = false;
        private Vector3 _initialScale;
        private float _baseEmissionIntensity;
        
        private void Awake()
        {
            _rend = GetComponent<Renderer>();
            _mainCamera = Camera.main;
            
            // 初期値の保存
            _defaultFov = _mainCamera.fieldOfView;
            _initialScale = transform.localScale;
            _baseEmissionIntensity = _emissionIntensity;
            
            // Post-process Volumeから各エフェクト取得
            if (_postProcessingVolume != null)
            {
                _postProcessingVolume.profile.TryGet(out _vignette);
                _postProcessingVolume.profile.TryGet(out _colorAdjustments);
                _postProcessingVolume.profile.TryGet(out _depthOfField);
                _postProcessingVolume.profile.TryGet(out _lensDistortion);
            }
            
            // パーティクルシステムの初期設定
            if (_collectionParticle != null)
            {
                _collectionParticle.Stop();
                var main = _collectionParticle.main;
                main.startColor = _emissionColor;
            }
            
            if (_playerAbsorbParticle != null)
            {
                _playerAbsorbParticle.Stop();
                var main = _playerAbsorbParticle.main;
                main.startColor = _emissionColor;
            }
        }

        private void Start()
        {
            DefaultSetting();
        }
        
        
        /// <summary>
        /// デフォルトのアニメーション
        /// </summary>
        [ContextMenu("DefaultSetting")]
        public void DefaultSetting()
        {
            _isLockedOn = false;
            StartRotationAnimation(_defaultRotateDuration);
            SetMaterialColor(_emissionColor, _emissionIntensity);
            StartPulseAnimation();
            
            if (_ambientLightParticle != null && !_ambientLightParticle.isPlaying)
            {
                _ambientLightParticle.Play(); // パーティクルを生成
            }
            
            ResetPostProcessing(); // ポストプロセス効果をリセット
        }

        /// <summary>
        /// ロックオン中のエフェクト
        /// </summary>
        [ContextMenu("LockOn")]
        public void LockOn()
        {
            if (_isLockedOn) return;
            _isLockedOn = true;
            
            // トゥイーンシーケンスを作成
            _lockOnEffectSequence = DOTween.Sequence();
            
            // 回転速度変更
            StartRotationAnimation(_lockOnRotationDuration);
            
            // 発光色と明滅を強化
            SetMaterialColor(_lockOnColor, _baseEmissionIntensity * 1.5f);
            
            // 明滅エフェクトを早くする
            if (_pulseEmissionTweener != null)
            {
                _pulseEmissionTweener.Kill();
            }
            StartPulseAnimation(_pulseSpeed * 2, _pulseMagnitude * 1.5f);
            
            // スケールを少し大きくする
            _lockOnEffectSequence.Append(transform.DOScale(_initialScale * 1.15f, 0.3f).SetEase(Ease.OutBack));
            
            // 画面効果
            ApplyLockOnPostProcessing();
            
            // 音響効果
            ApplyLockOnAudioEffects();
        }

        /// <summary>
        /// 回収時
        /// </summary>
        [ContextMenu("Get")]
        public void Get()
        {
            // トゥイーンを全て終了
            DOTween.Kill(transform);
            if (_pulseEmissionTweener != null) _pulseEmissionTweener.Kill();
            if (_lockOnEffectSequence != null) _lockOnEffectSequence.Kill();
            
            // パーティクルを止める
            if (_ambientLightParticle != null)
            {
                _ambientLightParticle.Stop();
            }
            
            // 回収エフェクトを再生
            PlayCollectionEffects();
            
            // 一瞬明るく光らせる
            _rend.material.DOColor(_lockOnColor * _baseEmissionIntensity * 3, "_EmissionColor", 0.1f)
                .OnComplete(() => {
                    // オブジェクトを非表示にする（パーティクルエフェクトが終わった後で消す）
                    StartCoroutine(DelayedDestroy(2.0f));
                    _rend.enabled = false;
                });
            
            // 画面効果
            ApplyCollectionPostProcessing();
            
            // 音響効果
            ApplyCollectionAudioEffects();
        }
        
        /// <summary>
        /// 一定時間後にオブジェクトを破棄するコルーチン
        /// </summary>
        private IEnumerator DelayedDestroy(float delay)
        {
            yield return new WaitForSeconds(delay);
            Destroy(gameObject);
        }

        /// <summary>
        /// オブジェクトを回転させるTween
        /// </summary>
        private void StartRotationAnimation(float duration)
        {
            _rotateTweener.Kill(); // 現在の回転Tweenを止める
            _rotateTweener = transform.DORotate(new Vector3(0f, 360f, 0f), duration, RotateMode.WorldAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
        }

        /// <summary>
        /// Materialの色を変更する
        /// </summary>
        private void SetMaterialColor(Color color, float intensity)
        {
            _rend.sharedMaterial.SetColor("_EmissionColor", color * intensity);
        }
        
        /// <summary>
        /// 明滅アニメーションを開始
        /// </summary>
        private void StartPulseAnimation(float speed = 0, float magnitude = 0)
        {
            float useSpeed = speed > 0 ? speed : _pulseSpeed;
            float useMagnitude = magnitude > 0 ? magnitude : _pulseMagnitude;
            
            // 現在の発光色と強度を取得
            Color currentColor = _rend.material.GetColor("_EmissionColor");
            float currentIntensity = currentColor.maxColorComponent;
            
            // 明滅エフェクト(SinカーブでEmissionの強度を変える)
            _pulseEmissionTweener = DOTween.To(
                () => currentIntensity,
                x => {
                    Color c = _isLockedOn ? _lockOnColor : _emissionColor;
                    _rend.material.SetColor("_EmissionColor", c * x);
                },
                currentIntensity * (1 - useMagnitude),
                1f / useSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        
        /// <summary>
        /// 回収時のエフェクトを再生
        /// </summary>
        private void PlayCollectionEffects()
        {
            // 爆発エフェクトの再生
            if (_collectionParticle != null)
            {
                _collectionParticle.Play();
            }
            
            // プレイヤーへの吸収エフェクト
            if (_playerAbsorbParticle != null)
            {
                // ターゲットプレイヤーの位置を取得
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    // パーティクルシステムの設定を調整
                    var main = _playerAbsorbParticle.main;
                    // 加速度を設定
                    var velocityOverLifetime = _playerAbsorbParticle.velocityOverLifetime;
                    velocityOverLifetime.enabled = true;
                    
                    // パーティクルの方向をプレイヤーに向ける
                    Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
                    velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(directionToPlayer.x * 5f, directionToPlayer.x * 10f);
                    velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(directionToPlayer.y * 5f, directionToPlayer.y * 10f);
                    velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(directionToPlayer.z * 5f, directionToPlayer.z * 10f);
                    
                    _playerAbsorbParticle.Play();
                }
            }
            
            // 効果音再生
        }
        
        #region ポストプロセス効果
        
        /// <summary>
        /// ロックオン時のポストプロセス効果を適用
        /// </summary>
        private void ApplyLockOnPostProcessing()
        {
            if (_postProcessingVolume == null) return;
            
            // ビネットエフェクト（画面端を暗く）
            if (_vignette != null)
            {
                DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x, 0.3f, 0.5f);
            }
            
            // FOV変更（ズームイン）
            if (_mainCamera != null)
            {
                DOTween.To(() => _mainCamera.fieldOfView, x => _mainCamera.fieldOfView = x, 
                    _defaultFov - _lockOnFovChange, 0.5f).SetEase(Ease.OutQuad);
            }
            
            // 色彩の飽和度を上昇
            if (_colorAdjustments != null)
            {
                DOTween.To(() => _colorAdjustments.saturation.value, x => _colorAdjustments.saturation.value = x, 
                    10f, 0.5f).SetEase(Ease.OutQuad);
            }
            
            // 被写界深度効果（背景をぼかす）
            if (_depthOfField != null)
            {
                _depthOfField.active = true;
                DOTween.To(() => _depthOfField.focusDistance.value, x => _depthOfField.focusDistance.value = x, 
                    1.5f, 0.5f).SetEase(Ease.OutQuad);
            }
        }
        
        /// <summary>
        /// 回収時のポストプロセス効果を適用
        /// </summary>
        private void ApplyCollectionPostProcessing()
        {
            if (_postProcessingVolume == null) return;
            
            // FOVを一瞬だけ縮小した後、元に戻す
            if (_mainCamera != null)
            {
                DOTween.Sequence()
                    .Append(DOTween.To(() => _mainCamera.fieldOfView, x => _mainCamera.fieldOfView = x, 
                        _defaultFov - _collectionFovPulse, 0.1f).SetEase(Ease.OutQuad))
                    .Append(DOTween.To(() => _mainCamera.fieldOfView, x => _mainCamera.fieldOfView = x, 
                        _defaultFov, 0.3f).SetEase(Ease.InOutBack));
            }
            
            // カメラに軽い衝撃波効果（揺れ）
            if (_mainCamera != null && _mainCamera.transform != null)
            {
                _mainCamera.transform.DOShakePosition(_cameraShakeDuration, _cameraShakeIntensity);
            }
            
            // レンズフレア効果（レンズディストーション）
            if (_lensDistortion != null)
            {
                DOTween.Sequence()
                    .Append(DOTween.To(() => _lensDistortion.intensity.value, x => _lensDistortion.intensity.value = x, 
                        0.2f, 0.1f).SetEase(Ease.OutQuad))
                    .Append(DOTween.To(() => _lensDistortion.intensity.value, x => _lensDistortion.intensity.value = x, 
                        0f, 0.3f).SetEase(Ease.InOutBack));
            }
            
            // ビネットと色彩効果を元に戻す
            ResetPostProcessing(0.5f);
        }
        
        /// <summary>
        /// ポストプロセス効果をリセット
        /// </summary>
        private void ResetPostProcessing(float duration = 0.3f)
        {
            if (_postProcessingVolume == null) return;
            
            // ビネットリセット
            if (_vignette != null)
            {
                DOTween.To(() => _vignette.intensity.value, x => _vignette.intensity.value = x, 0f, duration);
            }
            
            // FOVリセット
            if (_mainCamera != null)
            {
                DOTween.To(() => _mainCamera.fieldOfView, x => _mainCamera.fieldOfView = x, 
                    _defaultFov, duration).SetEase(Ease.OutQuad);
            }
            
            // 色彩リセット
            if (_colorAdjustments != null)
            {
                DOTween.To(() => _colorAdjustments.saturation.value, x => _colorAdjustments.saturation.value = x, 
                    0f, duration).SetEase(Ease.OutQuad);
            }
            
            // 被写界深度リセット
            if (_depthOfField != null)
            {
                DOTween.To(() => _depthOfField.focusDistance.value, x => _depthOfField.focusDistance.value = x, 
                    10f, duration).SetEase(Ease.OutQuad)
                    .OnComplete(() => _depthOfField.active = false);
            }
            
            // レンズディストーションリセット
            if (_lensDistortion != null)
            {
                DOTween.To(() => _lensDistortion.intensity.value, x => _lensDistortion.intensity.value = x, 
                    0f, duration).SetEase(Ease.OutQuad);
            }
        }
        
        #endregion
        
        #region 音響効果
        
        /// <summary>
        /// ロックオン時の音響効果を適用
        /// </summary>
        private void ApplyLockOnAudioEffects()
        {
            // BGMを減速
            // ローパスフィルター適用（くぐもった音質）
            // リバーブ（エコー）適用
        }
        
        /// <summary>
        /// 回収時の音響効果を適用
        /// </summary>
        private void ApplyCollectionAudioEffects()
        {
            // 音響効果をリセット（少し遅延させる）
            DOVirtual.DelayedCall(0.2f, () => ResetAudioEffects(0.5f));
        }
        
        /// <summary>
        /// 音響効果をリセット
        /// </summary>
        private void ResetAudioEffects(float duration = 0.3f)
        {
            // BGMのピッチを元に戻す
            // ローパスフィルターをリセット
            // リバーブをリセット
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // 全てのTweenを終了
            DOTween.Kill(transform);
            if (_pulseEmissionTweener != null) _pulseEmissionTweener.Kill();
            if (_lockOnEffectSequence != null) _lockOnEffectSequence.Kill();
            
            // ポストプロセス効果をリセット
            ResetPostProcessing(0f);
            ResetAudioEffects(0f);
        }
    }
}
