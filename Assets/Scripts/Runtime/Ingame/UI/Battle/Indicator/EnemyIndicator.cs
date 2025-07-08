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
            
            // UIのリセット
            ResetRingsScale();
            ResetRingsColor(_defaultColor);
         
            base.End();
            
            // 全てのTweenを停止
            _blinkSequence?.Kill();
            _contractionSequence?.Kill();
            _successSequence?.Kill();
            _failSequence?.Kill();
            
            // イベント購読解除
            _player.OnSuccessAvoid -= OnPlayerAvoidSuccess;
            _player.OnFailedAvoid -= PlayFailEffect;

            //敵攻撃はノックバックを与えるので確認
            _chartRingManager.CheckAllRingIndicatorRemainTime();
        }

        private const float CONTRACTION_SPEED = 2; // 収縮にかける拍

        [SerializeField] private Image _blurImage; // ブラーリング
        [SerializeField] private CanvasGroup _textCanvasGroup; // テキストの親オブジェクトのCanvasGroup
        
        [Header("色設定")]
        [SerializeField] private Color _warningColor = Color.red;
        [SerializeField] private Color _successColor = Color.yellow;
        [SerializeField] private Color _defaultColor = Color.white;

        [SerializeField] private float _blinkDuration = 0.2f;
        [SerializeField] private float _fadeDuration = 0.3f;
        
        private DG.Tweening.Sequence _blinkSequence; // 点滅アニメーションのシーケンス
        private DG.Tweening.Sequence _contractionSequence; // 縮小アニメーションのシーケンス
        private DG.Tweening.Sequence _successSequence; // 回避成功アニメーションのシーケンス
        private DG.Tweening.Sequence _failSequence; // 回避失敗アニメーションのシーケンス

        private bool _isFailed; // 回避失敗フラグ

        private void OnDestroy()
        {
            End();
        }
        
        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        private void InitializeComponents()
        {
            // 生成時点のタイミングを保存
            _timing = MusicEngineHelper.GetBeatSinceStart();
            
            // Tweenの配列を作成
            _tweens = new Tween[3];

            // serializeFieldで割り当てがされていなかったら、子オブジェクトから該当のコンポーネントを取得する
            if (_blurImage == null)
            {
                _blurImage = transform.GetChild(1).GetComponent<Image>();
            }

            if (_textCanvasGroup == null)
            {
                _textCanvasGroup = transform.GetChild(2).GetComponent<CanvasGroup>();
            }
            
            // 初期状態の設定
            ResetRingsScale();
            ResetRingsColor(_defaultColor);
            _textCanvasGroup.alpha = 1;
        }

        /// <summary>
        /// 1拍目　点滅シーケンス
        /// </summary>
        private void StartBlinkEffect()
        {
            // 既存の点滅エフェクトをキャンセル
            _blinkSequence?.Kill();
            _blinkSequence = DOTween.Sequence();

            // 3回点滅
            _blinkSequence.Append(_ringImage.DOColor(_warningColor, _blinkDuration).SetLoops(3, LoopType.Yoyo));
            _blinkSequence.Join(_selfImage.DOColor(_warningColor, _blinkDuration).SetLoops(3, LoopType.Yoyo));

            // デフォルト色に戻す
            _blinkSequence.Append(_ringImage.DOColor(_defaultColor, 0.2f).SetEase(Ease.OutQuint));
            _blinkSequence.Join(_selfImage.DOColor(_defaultColor, 0.2f).SetEase(Ease.OutQuint));

            // Tweenを配列に保存
            if (_tweens != null && _tweens.Length > 0)
            {
                _tweens[0] = _blinkSequence;
            }
        }

        /// <summary>
        /// 3拍目　リングの縮小
        /// </summary>
        private void StartContractionEffect()
        {
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;

            // 既存の縮小エフェクトをキャンセル
            _contractionSequence?.Kill();
            _contractionSequence = DOTween.Sequence();
            
            // 縮小エフェクト
            _contractionSequence.Append(_ringImage.rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear));

            // Tweenを配列に保存
            if (_tweens != null && _tweens.Length > 1)
            {
                _tweens[1] = _contractionSequence;
            }
        }

        /// <summary>
        /// プレイヤーが回避に成功したときに再生する回避成功エフェクト
        /// </summary>
        private void OnPlayerAvoidSuccess()
        {
            // 他のエフェクトが再生されていたらキャンセル
            _blinkSequence?.Kill();
            _contractionSequence?.Kill();
            // NOTE: 回避失敗のTweenとは同時に存在しないはずなので現状Killしていない

            // もし自分も再生中のTweenを持っていたらキルする
            _successSequence?.Kill();
            _successSequence = DOTween.Sequence();

            // パンチスケール
            _successSequence.Append(_selfImage.rectTransform.DOPunchScale(Vector3.one * 0.3f, _blinkDuration, 2, 0.5f));

            // 色変更とフェードアウト
            _successSequence.Join(CreateColorChangeSequence(_successColor, _fadeDuration));

            // 少し待機して成功の色変化を見せてからフェードアウトする
            _successSequence.AppendInterval(0.1f);
            _successSequence.Append(CreateFadeSequence(_fadeDuration));

            // エフェクトが完了したらEnd処理を実行
            _successSequence.OnComplete(End);

            // Tweenを配列に保存
            if (_tweens != null && _tweens.Length > 2)
            {
                _tweens[2] = _successSequence;
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
            _blinkSequence?.Kill();
            _contractionSequence?.Kill();
            // NOTE: 回避成功のTweenとは同時に存在しないはずなので現状Killしていない
            
            _failSequence?.Kill();
            _failSequence = DOTween.Sequence();
            
            // 色変更とフェードアウト
            _failSequence.Append(CreateColorChangeSequence(Color.darkGray, _fadeDuration));
            _failSequence.Join(CreateFadeSequence(_fadeDuration));
            
            _failSequence.OnComplete(End);
        }

        /// <summary>
        /// 各リングの拡大率をリセット
        /// </summary>
        private void ResetRingsScale()
        {
            if (_ringImage != null) _ringImage.rectTransform.localScale = Vector3.one * _initialScale;
            if (_selfImage != null) _selfImage.rectTransform.localScale = Vector3.one;
        }
        
        /// <summary>
        /// 各リングの色を変更する
        /// </summary>
        private void ResetRingsColor(Color color)
        {
            if (_ringImage != null) _ringImage.color = color;
            if (_selfImage != null) _selfImage.color = color;
            if (_blurImage != null) _blurImage.color = color;
        }
        
        /// <summary>
        /// フェードアウトシーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateFadeSequence(float duration)
        {
            var fadeSequence = DOTween.Sequence();
            
            if (_ringImage != null) fadeSequence.Join(_ringImage.DOFade(0f, duration).SetEase(Ease.OutQuint));
            if (_selfImage != null) fadeSequence.Join(_selfImage.DOFade(0f, duration).SetEase(Ease.OutQuint));
            if (_blurImage != null) fadeSequence.Join(_blurImage.DOFade(0f, duration).SetEase(Ease.OutQuint));
            if (_textCanvasGroup != null) fadeSequence.Join(_textCanvasGroup.DOFade(0f, duration).SetEase(Ease.OutQuint));
            
            return fadeSequence;
        }
        
        /// <summary>
        /// 色変更シーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateColorChangeSequence(Color targetColor, float duration)
        {
            var colorSequence = DOTween.Sequence();
            
            if (_ringImage != null) colorSequence.Join(_ringImage.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            if (_selfImage != null) colorSequence.Join(_selfImage.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            if (_blurImage != null) colorSequence.Join(_blurImage.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            
            return colorSequence;
        }
    }
}
