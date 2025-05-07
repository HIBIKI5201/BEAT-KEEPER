using System.Collections;
using DG.Tweening;
using SymphonyFrameWork.System;
using UnityEngine;

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
        [SerializeField] private float _lockOnFovChange = 10f; // ロックオン時のFOV変化量
        [SerializeField] private float _collectionFovPulse = 45f; // 回収時のFOV
        [SerializeField] private float _cameraShakeIntensity = 0.2f; // カメラ振動の強さ
        [SerializeField] private float _cameraShakeDuration = 0.2f; // カメラ振動の時間
        
        private VolumeController _volumeController;
        
        private Renderer _rend;
        private Tweener _rotateTweener;
        private Tweener _pulseEmissionTweener;
        private Sequence _lockOnEffectSequence;
        
        private bool _isLockedOn = false;
        private Vector3 _initialScale;
        private float _baseEmissionIntensity;
        
        private void Awake()
        {
            // コンポーネントの参照を取得
            _rend = GetComponent<Renderer>();
            _volumeController = ServiceLocator.GetInstance<VolumeController>();
            if (_volumeController == null)
            {
                Debug.LogWarning("[BeatCrystalController] VolumeControllerが見つかりません");
            }
            
            // 初期値の保存
            _initialScale = transform.localScale;
            _baseEmissionIntensity = _emissionIntensity;
            
            // パーティクルシステムの初期設定
            if (_collectionParticle != null) _collectionParticle.Stop();
            if (_playerAbsorbParticle != null) _playerAbsorbParticle.Stop();
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
            
            // オブジェクトのデフォルト演出を再生
            StartRotationAnimation(_defaultRotateDuration);
            SetMaterialColor(_emissionColor, _emissionIntensity);
            StartPulseAnimation();
            
            // ポストプロセスをリセット
            _volumeController?.ApplyPreset(EffectPresetEnum.Default, 0.3f);
            
            if (_ambientLightParticle != null && !_ambientLightParticle.isPlaying)
            {
                _ambientLightParticle.Play(); // パーティクルを生成
            }
        }

        /// <summary>
        /// ロックオン中のエフェクト
        /// </summary>
        [ContextMenu("LockOn")]
        public void LockOn()
        {
            if (_isLockedOn) return;
            _isLockedOn = true;
            
            // オブジェクトのロックオン中の演出を再生
            StartRotationAnimation(_lockOnRotationDuration); // 回転速度変更
            SetMaterialColor(_lockOnColor, _baseEmissionIntensity * 1.5f); // 発光色と明滅を強化
            StartPulseAnimation(_pulseSpeed * 2, _pulseMagnitude * 1.5f); // 明滅エフェクトを早くする
            
            // スケールを少し大きくする(Tweenは回収時にKillされる)
            _lockOnEffectSequence = DOTween.Sequence();
            _lockOnEffectSequence.Append(transform.DOScale(_initialScale * 1.15f, 0.8f).SetEase(Ease.OutBack));
            
            // ポストプロセスの設定
            _volumeController.ApplyPreset(EffectPresetEnum.Focus, 0.5f, Ease.OutQuad);
            
            //TODO: SE、AudioMixerの処理
            ApplyLockOnAudioEffects(); 
        }

        /// <summary>
        /// 回収時
        /// </summary>
        [ContextMenu("Get")]
        public void Get()
        {
            // トゥイーンを終了
            _pulseEmissionTweener?.Kill();
            _lockOnEffectSequence?.Kill();
            
            // パーティクルを止める
            _ambientLightParticle?.Stop(); 
            
            // 回収エフェクトを再生
            PlayCollectionEffects();
            
            // 一瞬明るく光らせる
            _rend.material.DOColor(_lockOnColor * _baseEmissionIntensity * 3, "_EmissionColor", 0.1f)
                .OnComplete(() => {
                    // オブジェクトを非表示にする（パーティクルエフェクトが終わった後で消す）
                    StartCoroutine(DelayedDestroy(2.0f));
                    _rend.enabled = false;
                });

            ApplyCollectionPostProcessing();
            
            //TODO: SE、AudioMixerの処理
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
            if (_pulseEmissionTweener != null)
            {
                _pulseEmissionTweener.Kill(); // 既にTweenがあったらKillしておく
            }
            
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
        }

        /// <summary>
        /// 回収時のポストプロセス効果を適用
        /// </summary>
        private void ApplyCollectionPostProcessing()
        {
            if (_volumeController == null)
                return;

            // フラッシュプリセットを一瞬適用
            _volumeController.ApplyPreset(EffectPresetEnum.Flash, 0.1f, Ease.OutQuad);

            // レンズディストーション効果を一瞬かける
            _volumeController.AdjustEffect(EffectTypeEnum.LensDistortion, 0.2f, 0.1f, Ease.OutQuad);

            // すぐに元に戻す
            DOVirtual.DelayedCall(0.1f, () =>
            {
                _volumeController.AdjustEffect(EffectTypeEnum.LensDistortion, 0f, 0.3f, Ease.InOutBack);
                _volumeController.ApplyPreset(EffectPresetEnum.Default, 0.5f, Ease.InOutQuad);
            });

            // FOVの調整
            _volumeController.AdjustEffect(EffectTypeEnum.CameraFov, _collectionFovPulse, 0.1f, Ease.OutQuad);
            
            _volumeController.CameraShake(_cameraShakeDuration, _cameraShakeIntensity);

        }

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
            
            // ポストプロセス効果をリセット
            _volumeController?.ResetAllEffects();
            
            ResetAudioEffects(0f);
        }
    }
}
