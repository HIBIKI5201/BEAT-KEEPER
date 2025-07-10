using BeatKeeper.Runtime.Ingame.System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     敵の攻撃警告・回避UI
    /// </summary>
    public class EnemyIndicator : RingIndicatorBase
    {
        public override int EffectLength => 5;

        /// <summary>
        /// ビートごとに実行される処理
        /// </summary>
        /// <param name="count"></param>
        public override void Effect(int count)
        {
            base.Effect(count);

            if (_isFailed)
            {
                // 回避失敗していたらreturn
                return;
            }

            switch (count)
            {
                // 1拍目　点滅して表示
                case 1:
                    InitializeComponents();
                    StartBlinkEffect();
                    break;

                // 3拍目　縮小エフェクトを開始する
                case 3:
                    _player.OnSuccessAvoid += OnPlayerAvoidSuccess;
                    _player.OnFailedAvoid += PlayFailEffect;
                    StartContractionEffect();
                    break;
            }
        }

        public override void End()
        {
            // フラグリセット
            _isFailed = false;

            ResetAllTween();
            
            // イベント購読解除
            _player.OnSuccessAvoid -= OnPlayerAvoidSuccess;
            _player.OnFailedAvoid -= PlayFailEffect;
            
            // UIのリセット
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
            
            base.End();

            //敵攻撃はノックバックを与えるので確認
            _chartRingManager.CheckAllRingIndicatorRemainTime();
        }

        private const float CONTRACTION_SPEED = 2; // 収縮にかける拍

        [SerializeField] private Image[] _ringImages;
        [SerializeField] private Image[] _translucentRingImages; // 半透明リング
        [SerializeField] private Text[] _centerTexts; // リング中央のテキスト
        
        [Header("追加の色設定")]
        [SerializeField] private Color _warningColor = Color.red;

        private bool _isFailed; // 回避失敗フラグ
        
        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        private void InitializeComponents()
        {
            // 生成時点のタイミングを保存
            _timing = MusicEngineHelper.GetBeatSinceStart();
            
            // Tweenの配列を作成
            _tweens = new Tween[3];
            
            // 初期状態の設定
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
        }

        /// <summary>
        /// 1拍目　点滅シーケンス
        /// </summary>
        private void StartBlinkEffect()
        {
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            var blinkSequence = DOTween.Sequence()

                // 3回点滅
                .Append(_ringImages[0].DOColor(_warningColor, _blinkDuration).SetLoops(3, LoopType.Yoyo))
            
                // デフォルト色に戻す
                .Append(_ringImages[0].DOColor(_defaultColor, 0.2f).SetEase(Ease.OutQuint));

            // Tweenを配列に保存
            if (_tweens != null && _tweens.Length > 0)
            {
                _tweens[0] = blinkSequence;
            }
        }

        /// <summary>
        /// 3拍目　リングの縮小
        /// </summary>
        private void StartContractionEffect()
        {
            // 点滅シーケンスをキル
            _tweens[0]?.Kill();
            
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            var contractionSequence = DOTween.Sequence();
            
            // 縮小エフェクト
            contractionSequence.Append(_ringImages[0].rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear));
            contractionSequence.Join(_translucentRingImages[0].rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear));

            // Tweenを配列に保存
            if (_tweens != null && _tweens.Length > 1)
            {
                _tweens[0] = contractionSequence;
            }
            
            // ブラーリングのパルス
            var blurPulseSequence = DOTween.Sequence()
                .Append(_ringImages[2].DOFade(_translucentDefaultColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                .Append(_ringImages[2].DOFade(_translucentDefaultColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .SetLoops(-1, LoopType.Restart);
            
            // Tweenを配列に保存
            if (_tweens != null && _tweens.Length > 1)
            {
                _tweens[1] = blurPulseSequence;
            }
        }

        /// <summary>
        /// プレイヤーが回避に成功したときに再生する回避成功エフェクト
        /// </summary>
        private void OnPlayerAvoidSuccess()
        {
            // 他のアニメーションが再生されていたらキャンセル
            ResetAllTween();

            var successSequence = DOTween.Sequence();

            // パンチスケール
            successSequence.Append(_ringImages[1].rectTransform.DOPunchScale(Vector3.one * 0.3f, _blinkDuration, 2, 0.5f));

            // 色変更とフェードアウト
            successSequence.Join(CreateColorChangeSequence(_successColor, _translucentSuccessColor, _fadeDuration));

            // 少し待機して成功の色変化を見せてからフェードアウトする
            successSequence.AppendInterval(0.1f);
            successSequence.Append(CreateFadeSequence(_fadeDuration));

            // エフェクトが完了したらEnd処理を実行
            successSequence.OnComplete(End);

            // Tweenを配列に保存
            if (_tweens != null && _tweens.Length > 2)
            {
                _tweens[2] = successSequence;
            }
        }

        /// <summary>
        /// プレイヤーが回避に失敗したときのアニメーション
        /// </summary>
        private void PlayFailEffect()
        {
            // 回避失敗フラグを立てる
            _isFailed = true;
            
            // 回避失敗したときに他のTweenが再生されていたら中断
            ResetAllTween();
            
            var failSequence = DOTween.Sequence();
            
            // 色変更とフェードアウト
            failSequence.Append(CreateColorChangeSequence(Color.darkGray, Color.darkGray, _fadeDuration));
            failSequence.Join(CreateFadeSequence(_fadeDuration));
            
            failSequence.OnComplete(End);
        }

        #region Reset

        /// <summary>
        /// Tweens配列をクリア
        /// </summary>
        private void ResetAllTween()
        {
            if (_tweens != null)
            {
                for (int i = 0; i < _tweens.Length; i++)
                {
                    _tweens[i] = null;
                }
            }
        }

        /// <summary>
        /// 各リングの拡大率をリセット
        /// </summary>
        private void ResetRingsScale()
        {
            if (_ringImages[0] != null) _ringImages[0].rectTransform.localScale = Vector3.one * _initialScale;
            if (_translucentRingImages[0] != null) _translucentRingImages[0].rectTransform.localScale = Vector3.one * _initialScale;
            if (_ringImages[1] != null) _ringImages[1].rectTransform.localScale = Vector3.one;
            if (_ringImages[2] != null) _ringImages[2].rectTransform.localScale = Vector3.one;
        }

        /// <summary>
        /// 各リングの色を変更する
        /// </summary>
        private void ResetRingsColor(Color color, Color translucentColor)
        {
            foreach (var ring in _ringImages)
            {
                ring.color = color;
            }
            foreach (var ring in _translucentRingImages)
            {
                ring.color = translucentColor;
            }
            foreach (var text in _centerTexts)
            {
                text.color = color;
            }
        }

        #endregion

        #region Create Sequence

        /// <summary>
        /// フェードアウトシーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateFadeSequence(float duration)
        {
            var fadeSequence = DOTween.Sequence();
            
            // リングのフェード
            foreach (var ring in _ringImages)
            {
                fadeSequence.Join(ring.DOFade(0f, duration).SetEase(Ease.Linear));
            }

            // 半透明リングのフェード
            foreach (var ring in _translucentRingImages)
            {
                fadeSequence.Join(ring.DOFade(0f, duration).SetEase(Ease.Linear));
            }
            
            // 中央のテキストのフェード
            foreach (var text in _centerTexts)
            {
                fadeSequence.Join(text.DOFade(0f, duration).SetEase(Ease.Linear));
            }
            
            return fadeSequence;
        }
        
        /// <summary>
        /// 色変更シーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateColorChangeSequence(Color targetColor, Color translucentColor,  float duration)
        {
            var colorSequence = DOTween.Sequence();
           
            // リングの色変更
            foreach (var ring in _ringImages)
            {
                colorSequence.Join(ring.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            }

            // 半透明リングの色変更
            foreach (var ring in _translucentRingImages)
            {
                colorSequence.Join(ring.DOColor(translucentColor, duration).SetEase(Ease.OutFlash));
            }
            
            // 中央のテキストのフェード
            foreach (var text in _centerTexts)
            {
                colorSequence.Join(text.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            }
            
            return colorSequence;
        }

        #endregion
    }
}
