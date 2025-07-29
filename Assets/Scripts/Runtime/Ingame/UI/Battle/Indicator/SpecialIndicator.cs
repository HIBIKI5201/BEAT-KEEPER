using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.System;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     スキルのインジケーターUI
    /// </summary>
    public class SpecialIndicator : RingIndicatorBase
    {
        public override int EffectLength => 3;
        
        public override void Effect(int count)
        {
            base.Effect(count);
            
            // 即座にスキルノーツ/フィニッシャーノーツの見た目の変更を行う
            CanvasChangeTween();
            
            // フィニッシャーが発動できるか監視を行う。前回の変更時と状態が変化したときだけ、見た目を切り替えるTweenを発火する
            StartAwaitable();
            
            switch (count)
            {
                // 1拍目　縮小エフェクトを開始する
                case 1:
                    InitializeComponents();
                    StartContractionEffect();
                    break;
                
                // 3拍目　スキル発動の演出を行う
                case 2:
                    _player.OnSkill += PlaySkillEffect;
                    break;
            }
        }
        
        public override void End()
        {
            _player.OnSkill -= PlaySkillEffect;
            
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            
            // 全てのTweenを停止
            foreach (var tween in _tweens)
            {
                tween?.Kill();
            }
            
            // NOTE: InitializeComponents()より先に表示されてしまうのでここでも初期化を行う
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
            CanvasChangeTween();
            
            base.End();
        }

        // Justタイミングは2拍後
        private const float CONTRACTION_SPEED = 2;
        // Justタイミングのあとの判定受付時間
        private const float RECEPTION_TIME = 0.45f;

        [Header("スキル/フィニッシャーノーツ切り替え用の設定")]
        [SerializeField] private CanvasGroup _skillGroup; // スキルノーツのCanvasGroup
        [SerializeField] private CanvasGroup _finisherGroup; // フィニッシャーノーツのCanvasGroup
        [SerializeField] private float _changeDuration = 0.2f; // 切り替えにかける秒数
        [SerializeField] private Color _finisherColor; // フィニッシャー用の色指定
        [SerializeField] private Color _translucentFinisherColor; // フィニッシャー用の半透明の色指定
        [SerializeField] Text _centerText;
        [SerializeField] private Image[] _ringImages;
        [SerializeField] private Image[] _translucentRingImages; // 半透明リング

        private CancellationTokenSource _cts; // フィニッシャー発動可能か監視する非同期処理のキャンセル用
        private bool _finisherable; // 前回の変更時フィニッシャー可能か
        private Tween _changeTween; // 切り替えTween
        
		// 譜面の長さ
        private int _chartLength => _chartRingManager.TargetData.ChartData.Chart.Length;

        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        private void InitializeComponents()
        {
            // 2種類のTweenを使用するため、配列も2つ分確保する
            _tweens = new Tween[2];
            
            // 初期化
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
        }
        
        /// <summary>
        /// リングの縮小
        /// </summary>
        private void StartContractionEffect()
        {
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;

            var sequence = DOTween.Sequence()
                
                // Just判定まで縮小を行う
                .Append(_ringImages[0].rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                .Join(_translucentRingImages[0].rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                .Join(_ringImages[3].rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                .Join(_translucentRingImages[1].rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                
                // 中央のリング
                .Join(_ringImages[1].rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                .Join(_ringImages[4].rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                
                // Just判定を過ぎたら縮小は続行しつつ段々フェードアウトする
                .Append(_ringImages[0].rectTransform.DOScale(Vector3.one * 0.5f, beatDuration * RECEPTION_TIME).SetEase(Ease.Linear))
                .Join(_ringImages[3].rectTransform.DOScale(Vector3.one * 0.5f, beatDuration * RECEPTION_TIME).SetEase(Ease.Linear))
                .Join(_translucentRingImages[0].rectTransform.DOScale(Vector3.one * 0.5f, beatDuration * RECEPTION_TIME).SetEase(Ease.Linear))
                .Join(_translucentRingImages[1].rectTransform.DOScale(Vector3.one * 0.5f, beatDuration * RECEPTION_TIME).SetEase(Ease.Linear))
                .Join(CreateFadeSequence(beatDuration * RECEPTION_TIME))
                
                // シーケンスが中断されなかった場合はミス。失敗演出を行う
                .OnComplete(() => PlayFailEffect());
            
            _tweens[0] = sequence;
            
            var 
            
            // ブラーリングのパルス
            var blurPulseSequence = DOTween.Sequence()
                
                // 色を濃くする
                .Append(_ringImages[2].DOFade(_translucentDefaultColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                .Join(_ringImages[5].DOFade(_translucentDefaultColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                .Join(_ringImages[6].DOFade(_translucentDefaultColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                
                // 元に戻る
                .Append(_ringImages[2].DOFade(_translucentDefaultColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .Join(_ringImages[5].DOFade(_translucentDefaultColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .Join(_ringImages[6].DOFade(_translucentDefaultColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .SetLoops(-1, LoopType.Restart);
            
            _tweens[1] = blurPulseSequence;
        }

        /// <summary>
        /// スキル発動エフェクト
        /// </summary>
        private void PlaySkillEffect()
        {
            if (MusicEngineHelper.GetBeatNearerSinceStart() % _chartLength != _timing)
            {
                // ノーツのタイミングより前なら処理はスキップ
                return;
            }   
            
            // 成功した場合はリングの縮小演出は不要になるのでキル
            _tweens[0]?.Kill();
           
            var successSequence = DOTween.Sequence();

            // パンチスケールと色変更
            successSequence.Append(_selfImage.rectTransform.DOPunchScale(Vector3.one * 0.65f, _blinkDuration, 2, 0.5f));
			successSequence.Join(CreateColorChangeSequence(_successColor, _translucentSuccessColor, _fadeDuration));            

            // フェードアウト
            successSequence.Append(CreateFadeSequence(_fadeDuration));

            // エフェクトが完了したらEnd処理を実行
            successSequence.OnComplete(End);

            _tweens[0] = successSequence;
        }

        /// <summary>
        /// 失敗演出
        /// </summary>
        private void PlayFailEffect()
        {
            // 念のためキルしておく
            _tweens[0]?.Kill();
            
            var failSequence = DOTween.Sequence();
            
            // 色変更とフェードアウト
            failSequence.Append(CreateColorChangeSequence(Color.darkGray, Color.darkGray, _fadeDuration));
            failSequence.Join(CreateFadeSequence(_fadeDuration));
            
            failSequence.OnComplete(End);
            
            _tweens[0] = failSequence;
        }

        private void StartAwaitable()
        {
            _cts = new CancellationTokenSource();
            _ = MonitorFinisherableStateAsync(_cts.Token);
        }
        
        private async Awaitable MonitorFinisherableStateAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_finisherable != _player.IsFinisherable())
                {
                    CanvasChangeTween();
                }
        
                await Awaitable.NextFrameAsync(token);
            }
        }
        
        /// <summary>
        /// フィニッシャーノーツとスキルノーツの見た目を切り替えるTween
        /// </summary>
        private void CanvasChangeTween()
        {
            // 変数を上書き
            _finisherable = _player.IsFinisherable();

            // 既にTweenがあったらKill
            _changeTween?.Kill();
            
            var sequence = DOTween.Sequence();
            
            if (_finisherable)
            {
                // フィニッシャーが可能なときはフィニッシャーノーツを表示する
                sequence.Append(_finisherGroup.DOFade(1, _changeDuration));
                sequence.Join(_skillGroup.DOFade(0, _changeDuration));
            }
            else
            {
                // フィニッシャーが不可能なときはスキルノーツを表示する
                sequence.Append(_skillGroup.DOFade(1, _changeDuration));
                sequence.Join(_finisherGroup.DOFade(0, _changeDuration));
            }

            _changeTween = sequence;

            StartAwaitable();
        }

        /// <summary>
        /// 各リングの拡大率を変更する
        /// </summary>
        private void ResetRingsScale()
        {
            if(_ringImages[0] != null) _ringImages[0].rectTransform.localScale = Vector3.one * _initialScale;
            if(_ringImages[3] != null) _ringImages[3].rectTransform.localScale = Vector3.one * _initialScale;
            if(_translucentRingImages[0] != null) _translucentRingImages[0].rectTransform.localScale = Vector3.one * _initialScale;
            if(_translucentRingImages[1] != null) _translucentRingImages[1].rectTransform.localScale = Vector3.one * _initialScale;
            if (_ringImages[1] != null) _ringImages[1].rectTransform.localScale = _centerRingsScale;
            if (_ringImages[4] != null) _ringImages[4].rectTransform.localScale = _centerRingsScale;
            if(_ringImages[2] != null) _ringImages[2].rectTransform.localScale = _centerRingsScale;
            if(_ringImages[5] != null) _ringImages[5].rectTransform.localScale = _centerRingsScale;
            if(_ringImages[6] != null) _ringImages[6].rectTransform.localScale = _centerRingsScale;
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
            
            if(_centerText != null) _centerText.color = color;
        }
        
        /// <summary>
        /// フェードアウトシーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateFadeSequence(float duration)
        {
            var fadeSequence = DOTween.Sequence();
            
            fadeSequence.Join(_ringImage.DOFade(0f, duration).SetEase(Ease.Linear));
            
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
            
            if(_centerText != null) fadeSequence.Join(_centerText.DOFade(0f, duration).SetEase(Ease.Linear));
            
            return fadeSequence;
        }
        
        /// <summary>
        /// 色変更シーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateColorChangeSequence(Color targetColor, Color translucentColor, float duration)
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
            
            // テキストの色変更
            if (_centerText != null) colorSequence.Join(_centerText.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            
            return colorSequence;
        }
    }
}