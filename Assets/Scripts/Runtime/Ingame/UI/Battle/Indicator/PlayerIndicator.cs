using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     プレイヤー攻撃のインジケーターUI
    /// </summary>
    public class PlayerIndicator : RingIndicatorBase
    {
        public override int EffectLength => 3;

        /// <summary>
        /// エフェクトを再生
        /// </summary>
        public override void Effect(int count)
        {
            base.Effect(count);

            switch (count)
            {
                // 1拍目　縮小エフェクトを開始する
                case 1:
                    InitializeComponents();
                    StartContractionEffect();
                    break;
                
                // 2拍目　念のため判定が始まる2拍目のタイミングでアクション登録を行う
                case 2:
                    _player.OnPerfectAttack += HandlePerfectAttack;
                    _player.OnGoodAttack += HandleGoodAttack;
                    break;
            }
        }

        public override void End()
        {
            _player.OnPerfectAttack -= HandlePerfectAttack;
            _player.OnGoodAttack -= HandleGoodAttack;

			base.End();
            
            // NOTE: InitializeComponents()より先に表示されてしまうのでここでも初期化を行う
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
        }

        // Justタイミングは2拍後
        private const float CONTRACTION_SPEED = 2;
        // Justタイミングのあとの判定受付時間 // TODO: PlayerDataから値をとってくるようにする
        private const float RECEPTION_TIME = 0.45f;

        [SerializeField] private Image[] _ringImages = new Image[2];
        // 譜面の長さ
        private int _chartLength => _chartRingManager.TargetData.ChartData.Chart.Length;

        private void Start()
        {
			_centerImage.enabled = true;
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
        }
        
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
                .Append(_ringImage.rectTransform.DOScale(Vector3.one, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                
                // Just判定を過ぎたら縮小は続行しつつ段々フェードアウトする
                .Append(_ringImage.rectTransform.DOScale(Vector3.one * 0.5f, beatDuration * RECEPTION_TIME).SetEase(Ease.Linear))
                .Join(CreateFadeSequence(beatDuration * RECEPTION_TIME))
                
                // シーケンスが中断されなかった場合はミス。失敗演出を行う
                .OnComplete(() => PlayFailEffect());
            
            _tweens[0] = sequence;
            
            // ブラーリングのパルス
            var blurPulseSequence = DOTween.Sequence()
                .Append(_ringImages[0].DOFade(_translucentDefaultColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                .Append(_ringImages[0].DOFade(_translucentDefaultColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .SetLoops(-1, LoopType.Restart);
            
            _tweens[1] = blurPulseSequence;
        }

        /// <summary>
        /// 当たりエフェクト（プレイヤーが攻撃に成功したときに再生）
        /// </summary>
        private void OnPlayerAttackSuccess(bool isPerfect)
        {
            if (MusicEngineHelper.GetBeatNearerSinceStart() % _chartLength != _timing)
            {
                // ノーツのタイミングより前なら処理はスキップ
                return;
            }

            // 成功した場合はリングの縮小演出は不要になるのでキル
            _tweens[0]?.Kill();
            
            if (isPerfect)
            {
                // パーフェクト判定の場合は収縮するリングのScaleを1に補正
                _ringImage.rectTransform.localScale = Vector3.one;   
            }
			HandleCenterImage(isPerfect);
           
            _centerImage.enabled = true;
            
            var successSequence = DOTween.Sequence();

            // パンチスケールと色変更
            successSequence.Append(_selfImage.rectTransform.DOPunchScale(Vector3.one * 0.65f, _blinkDuration, 2, 0.5f));
            successSequence.Join(CreateColorChangeSequence(_newColor, _translucentDefaultColor, _fadeDuration));
            
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
            _tweens[0]?.Kill();
            
			SetMissImage();
            
            var failSequence = DOTween.Sequence();
            
            // 色変更とフェードアウト
            failSequence.Append(CreateColorChangeSequence(Color.darkGray, Color.darkGray, _fadeDuration));
            failSequence.Join(CreateFadeSequence(_fadeDuration));
            
            failSequence.OnComplete(End);
            
            _tweens[0] = failSequence;
        }

		/// <summary>
        /// 各リングの拡大率を変更する
        /// </summary>
		private void ResetRingsScale()
		{
			if(_selfImage != null) _selfImage.rectTransform.localScale = Vector3.one;
			if(_ringImage != null) _ringImage.rectTransform.localScale = Vector3.one * _initialScale;
			if(_ringImages[0] != null) _ringImages[0].rectTransform.localScale = _centerRingsScale;
			if(_ringImages[1] != null) _ringImages[1].rectTransform.localScale = _centerRingsScale;
		}
        
        /// <summary>
        /// 各リングの色を変更する
        /// </summary>
        private void ResetRingsColor(Color color, Color translucentColor)
        {
            if(_ringImage != null) _ringImage.color = color;
            if(_ringImages[0] != null) _ringImages[0].color = color;
            if(_ringImages[1] != null) _ringImages[1].color = color;
        }
        
        /// <summary>
        /// フェードアウトシーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateFadeSequence(float duration)
        {
            var fadeSequence = DOTween.Sequence();
            
            if (_ringImage != null) fadeSequence.Join(_ringImage.DOFade(0f, duration).SetEase(Ease.Linear));
            if (_ringImages[0] != null) fadeSequence.Join(_ringImages[0].DOFade(0f, duration).SetEase(Ease.Linear));
            if (_ringImages[1] != null) fadeSequence.Join(_ringImages[1].DOFade(0f, duration).SetEase(Ease.Linear));
            
            return fadeSequence;
        }
        
        /// <summary>
        /// 色変更シーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateColorChangeSequence(Color targetColor, Color translucentColor, float duration)
        {
            var colorSequence = DOTween.Sequence();
            
            if (_ringImage != null) colorSequence.Join(_ringImage.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            if (_ringImages[0] != null) colorSequence.Join(_ringImages[0].DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            if (_ringImages[1] != null) colorSequence.Join(_ringImages[1].DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            
            return colorSequence;
        }
        
        private void HandlePerfectAttack() => OnPlayerAttackSuccess(true);
        private void HandleGoodAttack() => OnPlayerAttackSuccess(false);
    }
}
