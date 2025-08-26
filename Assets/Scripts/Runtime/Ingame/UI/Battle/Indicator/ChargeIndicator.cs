using BeatKeeper.Runtime.Ingame.System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    /// チャージ攻撃のインジケーターUI
    /// </summary>
    public class ChargeIndicator : RingIndicatorBase
    {
        public override int EffectLength => 5;

		public override void OnGet(Action onEndAction, Vector2 startPosition, Vector2 endPosition, int timing)
		{
			base.OnGet(onEndAction, startPosition, endPosition, timing);
            
			// 始点リングの位置を設定
			_startPositionRing.rectTransform.position = startPosition
                                                + new Vector2(Screen.width / 2, Screen.height / 2);

			// 終点リングの位置を設定
			_endPositionRing.rectTransform.position = endPosition
                                                + new Vector2(Screen.width / 2, Screen.height / 2);
		}

        /// <summary>
        /// エフェクトを再生
        /// </summary>
        public override void Effect(int count)
        {
            base.Effect(count);

            switch (count)
            {
                // 縮小エフェクトを開始する
                case 1:
                    StartContractionEffect();
                    break;
            }
        }
        
        public override void End()
        {
            base.End();
            
            // イベントの購読解除
            _player.OnStartChargeAttack -= OnPlayerCharge;
            _player.OnChargeAttack -= OnPlayerAttackSuccess;
            _player.OnMissChargeAttack -= PlayFailEffect;
            
            // Tweens配列をクリア
            if (_tweens != null)
            {
                for (int i = 0; i < _tweens.Length; i++)
                {
                    _tweens[i]?.Kill();
                }
            }
            
            ResetAllComponents();
        }
        
        private const float CONTRACTION_SPEED = 2; // 始点リングの収束にかける拍数
        private const float CHARGE_TIME = 2; // チャージにかかる拍
        private const float RECEPTION_TIME = 0.45f; // Justタイミングのあとの判定受付時間 // TODO: PlayerDataから値をとってくるようにする
        
        [Header("コンポーネントの参照")] 
		[SerializeField] private Image _startPositionRing; // 長押しの始めを示すリング
        [SerializeField] private Image _endPositionRing; // 長押しの終わりを示すリング

        [Header("追加の色設定")] 
        [SerializeField] private Color _chargeColor = Color.cyan;
        [SerializeField] private Color _criticalColor = Color.red;

		// 譜面の長さ
        private int _chartLength => _chartRingManager.TargetData.ChartData.Chart.Length;
        
        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        protected override void InitializeComponents()
        {
            ResetAllComponents();
            
            _tweens = new Tween[5];

            _player.OnStartChargeAttack += OnPlayerCharge; // チャージ開始
            _player.OnChargeAttack += OnPlayerAttackSuccess; // チャージ完了したあとに攻撃
            _player.OnMissChargeAttack += PlayFailEffect; // チャージ完了前に攻撃（=チャージ攻撃失敗）
        }

        #region ベースとなる演出
        
        /// <summary>
        /// 始点リングの縮小演出
        /// </summary>
        protected virtual void StartContractionEffect()
        {
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            // 表示
            SetAllAlpha(1f);
            
            var sequence = DOTween.Sequence()
                
                // 縮小開始（完全には収縮しきらないようにする）
                .Append(_ringImage.rectTransform.DOScale(_centerRingsScale, beatDuration * CONTRACTION_SPEED).SetEase(Ease.Linear))
                
                // Just判定後も縮小を続ける
                .Append(_ringImage.rectTransform.DOScale(_centerRingsScale, beatDuration * RECEPTION_TIME).SetEase(Ease.Linear))
                
                .OnComplete(() => PlayFailEffect());
            
            _tweens[0] = sequence;
            
            // ブラーリングのパルス
            var blurPulseSequence = DOTween.Sequence()
                .Append(_decorationImage.DOFade(_translucentDefaultColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                .Append(_decorationImage.DOFade(_translucentDefaultColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .SetLoops(-1, LoopType.Restart);
            
            _tweens[2] = blurPulseSequence;
        }

        #endregion

        public void OnPlayerChargeTutorial()
        {
            if (_tweens != null)
            {
                // 進行中の縮小以外の演出を停止。最終値に到達させた状態にする
                _tweens[0]?.Kill();
            }

            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            var totalDuration = beatDuration * CHARGE_TIME;

            // マスクのスケールを外側リングの大きさに合わせる
            _endPositionRing.rectTransform.localScale = _ringImage.rectTransform.localScale;

            var sequence = DOTween.Sequence()

                // 色変更（チャージ開始時）
                .Append(_ringImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 縮小するリング
                .Join(_decorationImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 縮小するリングの発光部分
                .Join(_startPositionRing.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 自身

                // メインのリング移動アニメーション（外側リングから内側リングへ）
                .Append(_ringImage.rectTransform.DOScale(_centerRingsScale, totalDuration * 0.9f).SetEase(Ease.OutQuart))

                // 必要に応じて位置も調整
                .Join(_startPositionRing.rectTransform.DOMove(_endPositionRing.transform.position, totalDuration * 0.9f).SetEase(Ease.Linear))

                .OnComplete(OnChargeComplete);

            _tweens[1] = sequence;
        }
        public void OnPlayerAttackSuccessTutorial()
        {
            // 他のすべてのTweenをキル
            for (int i = 0; i < _tweens.Length; i++)
            {
                _tweens[i]?.Kill();
            }
            
            HandleCenterImage(true);
            // 色とテキストが変更されていない場合、念のためここで変えておく
            ResetRingsColor(_newColor, _newColor);

            var sequence = DOTween.Sequence()

                // 拡大
                .Append(_startPositionRing.rectTransform.DOScale(_centerRingsScale * 1.05f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_ringImage.rectTransform.DOScale(Vector3.one * _initialScale * 1.05f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_decorationImage.rectTransform.DOScale(_centerRingsScale * 1.05f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))

                // フェードアウト
                .Join(CreateFadeSequence(_fadeDuration))

                .OnComplete(End);

            _tweens[3] = sequence;
        }

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

            if (_tweens != null)
            {
                // 進行中の縮小以外の演出を停止。最終値に到達させた状態にする
                _tweens[0]?.Kill();   
            }
            
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            var totalDuration = beatDuration * CHARGE_TIME;
    
            // マスクのスケールを外側リングの大きさに合わせる
            _endPositionRing.rectTransform.localScale = _ringImage.rectTransform.localScale;

            var sequence = DOTween.Sequence()
                
                // 色変更（チャージ開始時）
 	     	 	.Append(_ringImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 縮小するリング
	    	    .Join(_decorationImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 縮小するリングの発光部分
 	       		.Join(_startPositionRing.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 自身
        
        		// メインのリング移動アニメーション（外側リングから内側リングへ）
        		.Append(_ringImage.rectTransform.DOScale(_centerRingsScale, totalDuration * 0.9f).SetEase(Ease.OutQuart))
        
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
            ResetRingsColor(_newColor, _newColor);
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
            
            HandleCenterImage(true);
            // 色とテキストが変更されていない場合、念のためここで変えておく
            ResetRingsColor(_newColor, _newColor);
           
            var sequence = DOTween.Sequence()
                
                // 拡大
                .Append(_startPositionRing.rectTransform.DOScale(_centerRingsScale * 1.05f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_ringImage.rectTransform.DOScale(Vector3.one * _initialScale * 1.05f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_decorationImage.rectTransform.DOScale(_centerRingsScale * 1.05f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                
                // フェードアウト
                .Join(CreateFadeSequence(_fadeDuration))
                
                .OnComplete(End);

            _tweens[3] = sequence;
        }

        /// <summary>
        /// 失敗演出（チャージ完了前に終了）
        /// </summary>
        public override void PlayFailEffect()
        {
            for (int i = 0; i < 3; i++)
            {
                _tweens[i]?.Kill();
            }
            base.PlayFailEffect();
        }

        /// <summary>
        /// 全要素のアルファ値設定
        /// </summary>
        private void SetAllAlpha(float alpha)
        {
            if(_ringImage != null) _ringImage.color = new Color(_ringImage.color.r, _ringImage.color.g, _ringImage.color.b, alpha);
            if(_startPositionRing != null) _startPositionRing.color = new Color(_startPositionRing.color.r, _startPositionRing.color.g, _startPositionRing.color.b, alpha);
            if(_endPositionRing != null) _endPositionRing.color = new Color(_endPositionRing.color.r, _endPositionRing.color.g, _endPositionRing.color.b, alpha);
            if(_decorationImage != null) _decorationImage.color = new Color(_decorationImage.color.r, _decorationImage.color.g, _decorationImage.color.b, alpha);
            if(_centerImage != null) _centerImage.color = new Color(_centerImage.color.r, _centerImage.color.g, _centerImage.color.b, alpha);
        }
        
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
        
        #region リングのコンポーネント全てのScale、色のリセット
        
        /// <summary>
        /// 各リングの拡大率を変更する
        /// </summary>
        protected override void ResetRingsScale()
        {
            // 収縮する一番外側のリング
            if(_ringImage != null) _ringImage.rectTransform.localScale = Vector3.one * _initialScale;
            
            // チャージのゲージを管理しているもの
            if(_endPositionRing != null) _endPositionRing.rectTransform.localScale = Vector3.one * _initialScale;
            
            // 中央のUI
            if (_startPositionRing != null) _startPositionRing.rectTransform.localScale = _centerRingsScale;
            if (_decorationImage != null) _decorationImage.rectTransform.localScale = _centerRingsScale;
        }
        
        /// <summary>
        /// 各リングの色を変更する
        /// </summary>
        protected override void ResetRingsColor(Color color, Color translucentColor)
        {
            // NOTE: マスクの画像は色を変える必要がないのでここには書いていない
            if(_ringImage != null) _ringImage.color = color;
            if(_startPositionRing != null) _startPositionRing.color = color;
            if(_endPositionRing != null) _endPositionRing.color = color;
            if(_decorationImage != null) _decorationImage.color = color;
            if(_centerImage != null) _centerImage.color = Color.white;
        }

        #endregion

        #region シーケンス作成メソッド

        /// <summary>
        /// フェードアウトシーケンスを作成
        /// </summary>
        protected override DG.Tweening.Sequence CreateFadeSequence(float duration)
        {
            var fadeSequence = DOTween.Sequence();
            
            fadeSequence.Join(_ringImage.DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_startPositionRing.DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_endPositionRing.DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_decorationImage.DOFade(0f, duration).SetEase(Ease.Linear));
            fadeSequence.Join(_centerImage.DOFade(0f, duration).SetEase(Ease.Linear));
            
            return fadeSequence;
        }

        /// <summary>
        /// 色変更シーケンスを作成
        /// </summary>
        protected override DG.Tweening.Sequence CreateColorChangeSequence(Color targetColor, Color translucentColor,
            float duration)
        {
            var colorSequence = DOTween.Sequence();

            colorSequence.Join(_ringImage.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            colorSequence.Join(_startPositionRing.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            colorSequence.Join(_endPositionRing.DOColor(targetColor, duration).SetEase(Ease.OutFlash));
            colorSequence.Join(_decorationImage.DOColor(targetColor, duration).SetEase(Ease.OutFlash));

            return colorSequence;
        }

        #endregion

        #region Overrides for Tutorial

        public override void Pause()
        {
            base.Pause();
            if (_decorationImage != null)
            {
                // Kill the pulse animation and set the alpha to a static value.
                _tweens[2]?.Kill();
                var color = _decorationImage.color;
                color.a = _translucentDefaultColor.a;
                _decorationImage.color = color;
            }
        }

        public override void Resume()
        {
            base.Resume();
            if (_decorationImage != null)
            {
                // Recreate and play the pulse animation.
                var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
                var blurPulseSequence = DOTween.Sequence()
                    .Append(_decorationImage.DOFade(_translucentDefaultColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                    .Append(_decorationImage.DOFade(_translucentDefaultColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                    .SetLoops(-1, LoopType.Restart);
                _tweens[2] = blurPulseSequence;
            }
        }

        #endregion
    }
}
