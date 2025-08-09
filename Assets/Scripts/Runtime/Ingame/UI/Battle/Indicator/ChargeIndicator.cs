using BeatKeeper.Runtime.Ingame.System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    /// チャージ攻撃のインジケーターUI
    /// </summary>
    public class ChargeIndicator : RingIndicatorBase
    {
        public override int EffectLength => 5;

        /// <summary>
        /// エフェクトを再生
        /// </summary>
        public override void Effect(int count)
        {
            base.Effect(count);

            switch (count)
            {
                case 1:
                    InitializeComponents();
                    StartContractionEffect();
                    break;
            }
        }
        
        public override void End()
        {
            // イベントの購読解除
            _player.OnStartChargeAttack -= OnPlayerCharge;
            _player.OnFullChargeAttack -= OnPlayerAttackSuccess;
            _player.OnNonFullChargeAttack -= PlayFailEffect;
            
            // Tweens配列をクリア
            if (_tweens != null)
            {
                for (int i = 0; i < _tweens.Length; i++)
                {
                    _tweens[i]?.Kill();
                }
            }
            
            ResetAllComponents();
            
            base.End();
        }
        
        private const float CONTRACTION_SPEED = 2; // 始点リングの収束にかける拍数
        private const float CHARGE_TIME = 2; // チャージにかかる拍
        private const float RECEPTION_TIME = 0.45f; // Justタイミングのあとの判定受付時間 // TODO: PlayerDataから値をとってくるようにする
        
        // テキストコンポーネントで使用する文字列
        private const string DEFAULT_TEXT = "CHARGE";
        private const string DEFAULT_KEY_TEXT = "PRESS T KEY";
        private const string COMPLEAT_TEXT = "FIRE!!!";
        private const string COMPLEAT_KEY_TEXT = "RELEASE";
        
        [Header("コンポーネントの参照")] 
		[SerializeField] private Image _contractionImage;
        [SerializeField] private Image _blurImage; // 収縮を行う枠の発光演出用のリング
		[SerializeField] private Image _startPositionRing; // 長押しの始めを示すリング
        [SerializeField] private Image _endPositionRing; // 長押しの終わりを示すリング

        [Header("追加の色設定")] 
        [SerializeField] private Color _chargeColor = Color.cyan;
        [SerializeField] private Color _criticalColor = Color.red;

		// 譜面の長さ
        private int _chartLength => _chartRingManager.TargetData.ChartData.Chart.Length;

        private void Start()
        {
			_centerImage.enabled = true;
            ResetAllComponents();
        }
        
        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        private void InitializeComponents()
        {
            ResetAllComponents();
            
            _tweens = new Tween[5];

            _player.OnStartChargeAttack += OnPlayerCharge; // チャージ開始
            _player.OnFullChargeAttack += OnPlayerAttackSuccess; // チャージ完了したあとに攻撃
            _player.OnNonFullChargeAttack += PlayFailEffect; // チャージ完了前に攻撃（=チャージ攻撃失敗）
        }

        #region ベースとなる演出
        
        /// <summary>
        /// 始点リングの縮小演出
        /// </summary>
        private void StartContractionEffect()
        {
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            // 表示
            SetAllAlpha(1f);
            
            // ブラーリングのパルス
            var blurPulseSequence = DOTween.Sequence()
                .Append(_blurImage.DOFade(_translucentDefaultColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                .Append(_blurImage.DOFade(_translucentDefaultColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .SetLoops(-1, LoopType.Restart);
            
            _tweens[2] = blurPulseSequence;

            var sequence = DOTween.Sequence()
                
                // パンチアニメーション
                .Append(_startPositionRing.rectTransform.DOPunchScale(Vector3.one * 0.2f, beatDuration * 0.2f, 3, 0.8f))
                
                // 縮小開始（完全には収縮しきらないようにする）
                .Append(_contractionImage.rectTransform.DOScale(_centerRingsScale, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                
                // Just判定後も縮小を続ける
                .Append(_contractionImage.rectTransform.DOScale(_centerRingsScale, beatDuration * RECEPTION_TIME).SetEase(Ease.Linear))
                
                .OnComplete(() => PlayFailEffect());
            
            _tweens[0] = sequence;
        }

        #endregion

        /// <summary>
        /// チャージ中の演出
        /// </summary>
        private void OnPlayerCharge()
        {
			if (MusicEngineHelper.GetBeatNearerSinceStart() != _timing - CHARGE_TIME)
            {
                // ノーツのタイミングより前なら処理はスキップ
                return;
            }

            // 進行中の縮小以外の演出を停止。最終値に到達させた状態にする
            _tweens[0]?.Kill();
            
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            var totalDuration = beatDuration * CHARGE_TIME;
    
            // マスクのスケールを外側リングの大きさに合わせる
            _endPositionRing.rectTransform.localScale = _contractionImage.rectTransform.localScale;

            var sequence = DOTween.Sequence()
                
                // 色変更（チャージ開始時）
 	     	 	.Append(_contractionImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 縮小するリング
	    	    .Join(_blurImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 縮小するリングの発光部分
 	       		.Join(_startPositionRing.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 自身
        
        		// メインのリング移動アニメーション（外側リングから内側リングへ）
        		.Append(_contractionImage.rectTransform.DOScale(_centerRingsScale, totalDuration * 0.9f).SetEase(Ease.OutQuart))
        
       			// 必要に応じて位置も調整
        		.Join(_startPositionRing.rectTransform.DOMove(_endPositionRing.transform.position, totalDuration * 0.9f).SetEase(Ease.Linear))

		        .OnComplete(OnChargeComplete);
            
            _tweens[1] = sequence;
        }

        /// <summary>
        /// チャージ完了時の処理
        /// </summary>
        private void OnChargeComplete()
        {
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            // 色とテキストを変更
            ResetRingsColor(_chargeColor, _chargeColor);
        }
        
        /// <summary>
        /// 当たりエフェクト（プレイヤーが攻撃に成功したときに再生）
        /// </summary>
        private void OnPlayerAttackSuccess()
        {
            if (MusicEngineHelper.GetBeatNearerSinceStart() != _timing)
            {
                // ノーツのタイミングより前なら処理はスキップ
                return;
            }
            
            // 他のすべてのTweenをキル
            for (int i = 0; i < _tweens.Length; i++)
            {
                _tweens[i]?.Kill();
            }
            
            // 色とテキストが変更されていない場合、念のためここで変えておく
            ResetRingsColor(_chargeColor, _chargeColor);
           
            var sequence = DOTween.Sequence()
                
                // 拡大
                .Append(_startPositionRing.rectTransform.DOScale(_centerRingsScale * 1.5f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_contractionImage.rectTransform.DOScale(Vector3.one * _initialScale * 1.8f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_blurImage.rectTransform.DOScale(Vector3.one * _initialScale * 2f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                
                // フェードアウト
                .Join(CreateFadeSequence(_fadeDuration))
                
                .OnComplete(End);

            _tweens[3] = sequence;
        }

        /// <summary>
        /// 失敗演出（チャージ完了前に終了）
        /// </summary>
        private void PlayFailEffect()
        {
            for (int i = 0; i < 3; i++)
            {
                _tweens[i]?.Kill();
            }
            
            var sequence = DOTween.Sequence()
                
                .Append(_startPositionRing.rectTransform.DOScale(_centerRingsScale * 0.6f, _fadeDuration * 0.3f).SetEase(Ease.InQuad))
                .Join(_contractionImage.rectTransform.DOScale(Vector3.one * _initialScale * 0.7f, _fadeDuration * 0.3f).SetEase(Ease.InQuad))
                
                // フェードアウト
                .Append(CreateFadeSequence(_fadeDuration * 0.7f))
                .OnComplete(End);
            
            _tweens[0] = sequence;
        }

        #region Reset

        /// <summary>
        /// 全コンポーネントの完全初期化
        /// </summary>
        private void ResetAllComponents()
        {
            // スケールをリセット
            ResetRingsScale();
            
            // 色をデフォルトに設定
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
            
            SetAllAlpha(0f);

			// 移動するオブジェクトの位置を変更
			_startPositionRing.transform.position = transform.position;
        }
        
        /// <summary>
        /// 各リングの拡大率を変更する
        /// </summary>
        private void ResetRingsScale()
        {
            // 収縮する一番外側のリング
            if(_contractionImage != null) _contractionImage.rectTransform.localScale = Vector3.one * _initialScale;
            
            // チャージのゲージを管理しているもの
            if(_endPositionRing != null) _endPositionRing.rectTransform.localScale = Vector3.one * _initialScale;
            
            // 中央のUI
            if (_startPositionRing != null) _startPositionRing.rectTransform.localScale = _centerRingsScale;
        }
        
        /// <summary>
        /// 各リングの色を変更する
        /// </summary>
        private void ResetRingsColor(Color color, Color translucentColor)
        {
            // NOTE: マスクの画像は色を変える必要がないのでここには書いていない
            if(_contractionImage != null) _contractionImage.color = color;
            if(_startPositionRing != null) _startPositionRing.color = color;
            if(_endPositionRing != null) _endPositionRing.color = color;
            if(_blurImage != null) _blurImage.color = translucentColor;
        }

        /// <summary>
        /// 全要素のアルファ値設定
        /// </summary>
        private void SetAllAlpha(float alpha)
        {
            if(_contractionImage != null) _contractionImage.color = new Color(_contractionImage.color.r, _contractionImage.color.g, _contractionImage.color.b, alpha);
            if(_startPositionRing != null) _startPositionRing.color = new Color(_startPositionRing.color.r, _startPositionRing.color.g, _startPositionRing.color.b, alpha);
            if(_endPositionRing != null) _endPositionRing.color = new Color(_endPositionRing.color.r, _endPositionRing.color.g, _endPositionRing.color.b, alpha);
            if(_blurImage != null) _blurImage.color = new Color(_blurImage.color.r, _blurImage.color.g, _blurImage.color.b, alpha);
           
        }
        
        #endregion

        #region Create Tween

        /// <summary>
        /// フェードアウトシーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateFadeSequence(float duration)
        {
            var fadeSequence = DOTween.Sequence();
            
            fadeSequence.Join(_contractionImage.DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_startPositionRing.DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_endPositionRing.DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_blurImage.DOFade(0f, duration).SetEase(Ease.Linear));
            
            return fadeSequence;
        }
        
        /// <summary>
        /// 色変更シーケンスを作成
        /// </summary>
        private DG.Tweening.Sequence CreateColorChangeSequence(Color targetColor, Color translucentColor, float duration)
        {
            var colorSequence = DOTween.Sequence();
            
            colorSequence.Join(_contractionImage.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            colorSequence.Join(_startPositionRing.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            colorSequence.Join(_endPositionRing.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            colorSequence.Join(_blurImage.DOColor(translucentColor, duration).SetEase(Ease.OutFlash));
            
            return colorSequence;
        }
        
        #endregion
    }
}
