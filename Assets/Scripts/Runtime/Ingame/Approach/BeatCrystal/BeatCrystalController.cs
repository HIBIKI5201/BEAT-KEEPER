using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private float _collectionFovPulse = 45f; // 回収時のFOV
        [SerializeField] private float _cameraShakeIntensity = 0.2f; // カメラ振動の強さ
        [SerializeField] private float _cameraShakeDuration = 0.2f; // カメラ振動の時間
        
        private VolumeController _volumeController;
        private Renderer _rend;
        
        private Dictionary<string, Tween> _activeTweens = new Dictionary<string, Tween>(); // アクティブなアニメーション
        
        private bool _isLockedOn = false;
        private Vector3 _initialScale;
        private float _baseEmissionIntensity;
        
        private void Start()
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
            if(_collectionParticle != null) _collectionParticle.Stop();
            if(_playerAbsorbParticle != null) _playerAbsorbParticle?.Stop();
            if(_ambientLightParticle != null) _ambientLightParticle?.Play();
            
            ApplyDefaultEffects();
        }
        
        /// <summary>
        /// デフォルトの演出
        /// </summary>
        public void ApplyDefaultEffects()
        {
            _isLockedOn = false;
            
            // オブジェクトのデフォルト演出を再生
            StartRotationAnimation(_defaultRotateDuration, "rotation");
            SetMaterialColor(_emissionColor, _emissionIntensity);
            StartPulseAnimation(0, 0, "pulse");
            
            // ポストプロセスをリセット
            _volumeController?.ApplyPreset(EffectPresetEnum.Default, 0.3f);
            
            if (_ambientLightParticle != null && !_ambientLightParticle.isPlaying)
            {
                _ambientLightParticle.Play(); // パーティクルを生成
            }
        }

        /// <summary>
        /// ロックオン中の演出
        /// </summary>
        public void ApplyLockOnEffects()
        {
            if (_isLockedOn) return;
            _isLockedOn = true;
            
            StopTween("lockOnEffect");
            
            // オブジェクトのロックオン中の演出を再生
            StartRotationAnimation(_lockOnRotationDuration, "rotation"); // 回転速度変更
            SetMaterialColor(_lockOnColor, _baseEmissionIntensity * 1.5f); // 発光色と明滅を強化
            StartPulseAnimation(_pulseSpeed * 2, _pulseMagnitude * 1.5f, "pulse"); // 明滅エフェクトを早くする
            
            // スケールを少し大きくする(Tweenは回収時にKillされる)
            var scaleSequence = DOTween.Sequence();
            scaleSequence.Append(transform.DOScale(_initialScale * 1.15f, 1f).SetEase(Ease.OutBack));
            _activeTweens["lockOnEffect"] = scaleSequence;
            
            // ポストプロセスの設定
            _volumeController.ApplyPreset(EffectPresetEnum.Focus, 0.5f, Ease.OutQuad);
            
            ApplyLockOnAudioEffects();  //TODO: SE、AudioMixerの処理
        }

        /// <summary>
        /// 回収時の演出
        /// </summary>
        public void ApplyCollectionEffects()
        {
            StopAllTweens();
            if(_ambientLightParticle!= null) _ambientLightParticle.Stop(); 
            
            PlayCollectionEffects(); // 回収エフェクトを再生
            
            // 一瞬明るく光らせる
            _rend.material.DOColor(_lockOnColor * _baseEmissionIntensity * 3, "_EmissionColor", 0.1f)
                .OnComplete(() => {
                    // オブジェクトを非表示にする（パーティクルエフェクトが終わった後で消す）
                    StartCoroutine(DelayedDestroy(2.0f));
                    _rend.enabled = false;
                });

            ApplyCollectionPostProcessing(); // ポストプロセスの処理
            ApplyCollectionAudioEffects(); //TODO: SE、AudioMixerの処理
        }

        #region privateの処理

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
        private void StartRotationAnimation(float duration, string tweenId)
        {
            StopTween(tweenId); // 現在の回転Tweenを止める
            var rotateTweener = transform.DORotate(new Vector3(0f, 360f, 0f), duration, RotateMode.WorldAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1);
            _activeTweens[tweenId] = rotateTweener;
        }

        /// <summary>
        /// Materialの色を変更する
        /// </summary>
        private void SetMaterialColor(Color color, float intensity)
        {
            if (_rend == null || _rend.sharedMaterial == null) return;
            _rend.sharedMaterial.SetColor("_EmissionColor", color * intensity);
        }
        
        /// <summary>
        /// 明滅アニメーションを開始
        /// </summary>
        private void StartPulseAnimation(float speed, float magnitude, string tweenId)
        {
            if (_rend == null || _rend.material == null) return;
            
            StopTween(tweenId); // 既にTweenがあったらKillしておく
            
            // 現在の発光色と強度を取得
            Color currentColor = _rend.material.GetColor("_EmissionColor");
            float currentIntensity = currentColor.maxColorComponent;
            
            // 明滅エフェクト(SinカーブでEmissionの強度を変える)
            var pulseTweener = DOTween.To(
                () => currentIntensity,
                x => {
                    Color c = _isLockedOn ? _lockOnColor : _emissionColor;
                    _rend.material.SetColor("_EmissionColor", c * x);
                },
                currentIntensity * (1 - magnitude),
                1f / speed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
            _activeTweens[tweenId] = pulseTweener;
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
                    
            // プリセット適用        
            _volumeController.ApplyPreset(EffectPresetEnum.GetCrystal, 0.1f, Ease.OutQuad);
            
            _volumeController.AdjustEffect(EffectTypeEnum.LensDistortion, 0.2f, 0.1f, Ease.OutQuad); // レンズディストーション効果を一瞬かける
            _volumeController.AdjustEffect(EffectTypeEnum.CameraFov, _collectionFovPulse, 0.1f, Ease.OutQuad); // FOVの調整
            _volumeController.ApplyCameraShake(_cameraShakeDuration, _cameraShakeIntensity); // カメラシェイク

            // 効果を元に戻す
            DOVirtual.DelayedCall(0.1f, () =>
            {
                _volumeController.AdjustEffect(EffectTypeEnum.LensDistortion, 0f, 0.3f, Ease.InOutBack);
                _volumeController.ApplyPreset(EffectPresetEnum.Default, 0.5f, Ease.InOutQuad);
            });
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
        
        /// <summary>
        /// 指定した名前のTweenを停止する
        /// </summary>
        /// <param name="tweenId">停止するトゥイーンの識別子</param>
        private void StopTween(string tweenId)
        {
            if (_activeTweens.TryGetValue(tweenId, out Tween tween))
            {
                tween.Kill();
                _activeTweens.Remove(tweenId);
            }
        }
        
        /// <summary>
        /// 全てのTweenを停止する
        /// </summary>
        private void StopAllTweens()
        {
            foreach (var tween in _activeTweens.Values)
            {
                tween.Kill();
            }
            _activeTweens.Clear();
        }
        
        private void OnDestroy()
        {
            DOTween.Kill(transform); // 全てのTweenを終了
            _volumeController?.ApplyPreset(EffectPresetEnum.Default, 0.3f); // ポストプロセスをデフォルトに戻す
            ResetAudioEffects(0f);
        }

        #endregion
    }
}
