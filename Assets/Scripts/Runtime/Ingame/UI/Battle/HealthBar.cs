using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BeatKeeper.Runtime.Ingame.System;
using SymphonyFrameWork.System;

namespace BeatKeeper
{
    /// <summary>
    /// HPバーのアニメーション用クラス
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image _bar;
        [SerializeField] private Image _backBar;
        
        [Header("アニメーション設定")]
        [SerializeField] private Vector3 _defaultScale = Vector3.one; // デフォルトのScale
        [SerializeField, Tooltip("パルスの強さ")] private float _pulseIntensity = 1.1f;
        [SerializeField, Tooltip("ゲージ減少アニメーションにかける時間")] private float _decreaseAnimationDuration = 0.15f;
        [SerializeField, Tooltip("背景バー同期アニメーションにかける時間")] private float _backBarSyncDuration = 0.3f;
        [SerializeField, Tooltip("背景バー同期までの待機拍数")] private int _waitBeatsCount = 2;

        [SerializeField, Tooltip("通常時の色")] private Color _normalColor = Color.white;
        [SerializeField, Tooltip("フラッシュ時の色")] private Color _flashColor = Color.white;
        
        private Sequence _decreaseSequence; // HP減少シーケンス
        private Tween _backBarSyncTween; // 背景バー同期用Tween
        private BGMManager _bgmManager;
        
        // 状態管理用フラグ
        private bool _isValueChange; // 値が変更されたか
        private int _beatsToWait; // 待機する拍数（0で待機なし）
        private float _pendingBackBarTarget; // 背景バーの同期先の値

        #region Life cycle

        /// <summary>
        /// Start
        /// </summary>
        private async void Start()
        {
            _bgmManager = await ServiceLocator.GetInstanceAsync<BGMManager>();
            _bgmManager.OnJustChangedBeat += OnBeatChanged;
        }

        /// <summary>
        /// Destroy
        /// </summary>
        private void OnDestroy()
        {
            _decreaseSequence?.Kill();
            _backBarSyncTween?.Kill();
            
            if (_bgmManager != null)
            {
                _bgmManager.OnJustChangedBeat -= OnBeatChanged;
            }
        }

        #endregion
        
        /// <summary>
        /// HP減少アニメーションを実行する
        /// </summary>
        public void PlayAnimation(float targetFillAmount)
        {
            // 値変更を記録
            _isValueChange = true;
            
            // 背景バー同期処理をキャンセル（新しい攻撃が来た場合）
            CancelBackBarSync();
            
            var rectTransform = _bar.rectTransform;
            rectTransform.localScale = _defaultScale;
            
            // 念のためキル
            _decreaseSequence?.Kill();
            _decreaseSequence = DOTween.Sequence()
                // Y軸方向にパルス
                .Append(rectTransform.DOScaleY(_defaultScale.y * (1f + _pulseIntensity * 0.3f), 0.05f).SetEase(Ease.OutQuart))
                .Append(rectTransform.DOScaleY(_defaultScale.y, 0.1f).SetEase(Ease.OutQuart))

                // ヘルスバーの減少
                .Insert(0, _bar.DOFillAmount(targetFillAmount, _decreaseAnimationDuration).SetEase(Ease.OutExpo))

                // フラッシュ効果
                .Insert(0, _bar.DOColor(_flashColor, 0.05f).SetEase(Ease.OutQuart)
                    .OnComplete(() => _bar.DOColor(_normalColor, 0.15f).SetEase(Ease.OutQuart)));

            // 指定拍数後に背景バーを同期する準備
            _pendingBackBarTarget = targetFillAmount;
            _beatsToWait = _waitBeatsCount;
        }

        /// <summary>
        /// 拍に合わせて呼び出される処理
        /// </summary>
        private void OnBeatChanged()
        {
            if (_beatsToWait > 0)
            {
                _beatsToWait--;
                
                // 指定拍数経過したら背景バーを同期
                if (_beatsToWait == 0)
                {
                    SyncBackBarToCurrent();
                }
            }

            // 値変更フラグをリセット
            _isValueChange = false;
        }

        /// <summary>
        /// 背景バーを現在の_barのfillAmountに同期させる
        /// </summary>
        private void SyncBackBarToCurrent()
        {
            _backBarSyncTween?.Kill();
            _backBarSyncTween = _backBar.DOFillAmount(_bar.fillAmount, _backBarSyncDuration)
                .SetEase(Ease.OutQuart)
                .OnComplete(() => _backBar.fillAmount = _bar.fillAmount); // 完全に一致させる
        }

        /// <summary>
        /// 背景バーの同期処理をキャンセルする
        /// </summary>
        private void CancelBackBarSync()
        {
            if (_beatsToWait > 0 && _isValueChange)
            {
                _backBarSyncTween?.Kill();
                _beatsToWait = 0;
            }
        }
    }
}