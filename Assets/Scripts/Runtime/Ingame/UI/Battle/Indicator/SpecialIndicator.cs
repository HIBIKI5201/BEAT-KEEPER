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
            
            // フィニッシャーが発動できるか監視を行う。前回の変更時と状態が変化したときだけ、見た目を切り替えるTweenを発火する
            StartFinisherMonitoring();
            
            switch (count)
            {
                // 1拍目　縮小エフェクトを開始する
                case 1:
                    InitializeComponents();
                    StartContractionEffect();
                    break;
                
                // 3拍目　スキル発動の演出を行う
                case 2:
                    _player.OnSkill += PlaySuccessEffect;
                    break;
            }
        }
        
        public override void End()
        {
            _player.OnSkill -= PlaySuccessEffect;
            StopFinisherMonitoring();

            base.End();
            
            // NOTE: InitializeComponents()より先に表示されてしまうのでここでも初期化を行う
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
            UpdateRingState();
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
        private bool _isFinisherable; // フィニッシャー可能か
        private Color _currentPulseColor; // パルスの色の管理
		
        private int _chartLength => _chartRingManager.TargetData.ChartData.Chart.Length; // 譜面の長さ

        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        private void InitializeComponents()
        {
            // 2種類のTweenを使用するため、配列も2つ分確保する
            _tweens = new Tween[3];
            
            // 初期化
            ResetRingsScale();
            
            // 初回のフィニッシャー状態をチェックしてUI更新
            UpdateRingState();
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
                .Join(_ringImages[3].rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                .Join(_translucentRingImages[0].rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
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
            
            // ブラーリングのパルス
            var blurPulseSequence = DOTween.Sequence()
                
                // 色を濃くする
                .Append(_ringImages[2].DOFade(_currentPulseColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                .Join(_ringImages[5].DOFade(_currentPulseColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                .Join(_ringImages[6].DOFade(_currentPulseColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                
                // 元に戻る
                .Append(_ringImages[2].DOFade(_currentPulseColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .Join(_ringImages[5].DOFade(_currentPulseColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .Join(_ringImages[6].DOFade(_currentPulseColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .SetLoops(-1, LoopType.Restart);
            
            _tweens[1] = blurPulseSequence;
        }

        /// <summary>
        /// 発動エフェクト
        /// </summary>
        public void PlaySuccessEffectPublic()
        {
            _tweens[0]?.Kill();

            var successSequence = DOTween.Sequence();

            // パンチスケールと色変更
            successSequence.Append(_selfImage.rectTransform.DOPunchScale(Vector3.one * 0.65f, _blinkDuration, 2, 0.5f));
            successSequence.Join(CreateColorChangeSequence(_newColor, _newTranslucentColor, _fadeDuration));

            // フェードアウト
            successSequence.Append(CreateFadeSequence(_fadeDuration));

            // エフェクトが完了したらEnd処理を実行
            successSequence.OnComplete(End);

            _tweens[0] = successSequence;
        }

        /// <summary>
        /// 発動エフェクト
        /// </summary>
        private void PlaySuccessEffect()
        {
            if (MusicEngineHelper.GetBeatNearerSinceStart() != _timing)
            {
                // ノーツのタイミングより前なら処理はスキップ
                return;
            }   
            
            // 成功した場合はリングの縮小演出は不要になるのでキル
            _tweens[0]?.Kill();
           
            var successSequence = DOTween.Sequence();

            // パンチスケールと色変更
            successSequence.Append(_selfImage.rectTransform.DOPunchScale(Vector3.one * 0.65f, _blinkDuration, 2, 0.5f));
			successSequence.Join(CreateColorChangeSequence(_newColor, _newTranslucentColor, _fadeDuration));            

            // フェードアウト
            successSequence.Append(CreateFadeSequence(_fadeDuration));

            // エフェクトが完了したらEnd処理を実行
            successSequence.OnComplete(End);

            _tweens[0] = successSequence;
        }

        /// <summary>
        /// 失敗演出
        /// </summary>
        public void PlayFailEffect()
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

        #region スキル/フィニッシャーの切り替え
        
        /// <summary>
        /// フィニッシャー状態の監視を開始
        /// </summary>
        private void StartFinisherMonitoring()
        {
            // 既存の監視があれば停止
            StopFinisherMonitoring();
            
            // 新しく監視を始める
            _cts = new CancellationTokenSource();
            _ = MonitorFinisherableStateAsync(_cts.Token);
        }

        /// <summary>
        /// 既存の監視があれば停止
        /// </summary>
        private void StopFinisherMonitoring()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }
        
        /// <summary>
        /// フィニッシャー状態を非同期で監視
        /// </summary>
        private async Awaitable MonitorFinisherableStateAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_isFinisherable != _player.IsFinisherable())
                {
                    UpdateRingState();
                }
        
                await Awaitable.NextFrameAsync(token);
            }
        }
        
        /// <summary>
        /// ノーツの状態を更新
        /// </summary>
        private void UpdateRingState()
        {
            // 変数を上書き
            _isFinisherable = _player.IsFinisherable();

            SwitchCanvasGroup();
            ApplyCurrentColors();

            // もう一度監視用のAwaitableを行う
            StartFinisherMonitoring();
        }

        /// <summary>
        /// CanvasGroupの切り替え
        /// </summary>
        private void SwitchCanvasGroup()
        {
            // 既にTweenがあったらKill
            _tweens[2]?.Kill();
            
            var sequence = DOTween.Sequence();
            
            if (_isFinisherable)
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

            _tweens[2] = sequence;
        }
        
        /// <summary>
        /// 現在の状態に応じたパルスの色とリングの色を適用
        /// </summary>
        private void ApplyCurrentColors()
        {
            if (_isFinisherable)
            {
                _currentPulseColor = _translucentFinisherColor;
                ResetRingsColor(_finisherColor, _translucentFinisherColor);
            }
            else
            {
                _currentPulseColor = _translucentDefaultColor;
                ResetRingsColor(_defaultColor, _translucentDefaultColor);
            }
        }
        
        #endregion

        /// <summary>
        /// 各リングの拡大率を変更する
        /// </summary>
        private void ResetRingsScale()
        {
            // 収縮を行うリング
            SetRingScale(_ringImages[0], Vector3.one * _initialScale);
            SetRingScale(_ringImages[3], Vector3.one * _initialScale);
            
            // 収縮を行うリングのブラーImage
            SetRingScale(_translucentRingImages[0], Vector3.one * _initialScale);
            SetRingScale(_translucentRingImages[1], Vector3.one * _initialScale);
            
            // HitLine
            SetRingScale(_ringImages[1], _centerRingsScale);
            SetRingScale(_ringImages[4], _centerRingsScale);
            
            // 中央のデコレーションリング
            SetRingScale(_ringImages[2], _centerRingsScale);
            SetRingScale(_ringImages[5], _centerRingsScale);
            SetRingScale(_ringImages[6], _centerRingsScale);
        }
        
        /// <summary>
        /// 個別リングのスケール設定（nullチェック付き）
        /// </summary>
        private void SetRingScale(Image ring, Vector3 scale)
        {
            if (ring != null)
                ring.rectTransform.localScale = scale;
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