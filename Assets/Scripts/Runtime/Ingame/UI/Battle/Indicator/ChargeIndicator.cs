using BeatKeeper.Runtime.Ingame.System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;

using System.Collections;
using System.Collections.Generic;

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

            startPosition *= _resolutionMultiply;
            endPosition *= _resolutionMultiply;
            
			// 始点リングの位置を設定
			_startPositionRing.rectTransform.position = startPosition
                                                + new Vector2(Screen.width / 2, Screen.height / 2);

			// 終点リングの位置を設定
			_endPositionRing.rectTransform.position = endPosition
                                                + new Vector2(Screen.width / 2, Screen.height / 2);
		}
        
        public override void End()
        {
            base.End();
            
            ResetAllComponents();
        }
        
        public override void PlayFailEffect()
        {
            PlayFailEffectCharging();
        }
        
        #region チュートリアル用のメソッド
        
        /// <summary>
        /// チャージ演出
        /// </summary>
        public void OnPlayerChargeTutorial() => OnPlayerChargeForced();

        /// <summary>
        /// 長押し成功演出
        /// </summary>
        public void OnPlayerAttackSuccessTutorial() => OnPlayerSuccessForced(true);
        
        #endregion
        
        private const float CONTRACTION_SPEED = 2; // 始点リングの収束にかける拍数
        private const float CHARGE_TIME = 2; // チャージにかかる拍
        private const float RECEPTION_TIME = 0.45f; // Justタイミングのあとの判定受付時間 // TODO: PlayerDataから値をとってくるようにする
        
        [Header("コンポーネントの参照")] 
		[SerializeField] private Image _startPositionRing; // 長押しの始めを示すリング
        [SerializeField] private Image _endPositionRing; // 長押しの終わりを示すリング

        [Header("追加の色設定")] 
        [SerializeField] private Color _chargeColor = Color.cyan;

        [Header("追加の画像設定")]
        [SerializeField] private Sprite _defaultEndRingSprite;

		// 譜面の長さ
        private int _chartLength => _chartRingManager.TargetData.ChartData.Chart.Length;
        
        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        protected override void InitializeComponents()
        {
            ResetAllComponents();
            SetAllAlpha(1f);
            
            _tweens = new Tween[5];
        }

        #region チャージ開始（長押し中）の演出
        
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
            
            OnPlayerChargeForced();
        }

        /// <summary>
        /// 強制的にチャージ演出を実行
        /// </summary>
        private void OnPlayerChargeForced()
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

            // 色変更用にリングを白色のものに変更
            //ChargeStart();
            
            var sequence = DOTween.Sequence()
                
                // 色変更（チャージ開始時）NOTE: 戻す可能性があるのでコメントアウト
                // .Append(_ringImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 縮小するリング
                // .Join(_decorationImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 縮小するリングの発光部分
                // .Join(_startPositionRing.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 自身
        
                // メインのリング移動アニメーション（外側リングから内側リングへ）
                .Append(_ringImage.rectTransform.DOScale(_centerRingsScale, totalDuration).SetEase(Ease.Linear))
                
                // 終点リングへ移動
                .Join(_startPositionRing.rectTransform.DOMove(_endPositionRing.transform.position, totalDuration).SetEase(Ease.Linear))
                .Join(_centerImage.rectTransform.DOMove(_endPositionRing.transform.position, totalDuration).SetEase(Ease.Linear))

                .OnComplete(OnChargeComplete);
            
            _tweens[0] = sequence;
        }

        #endregion
        
        #region チャージ完了演出
        
        /// <summary>
        /// チャージ完了時の処理
        /// </summary>
        private void OnChargeComplete()
        {
            var beatDuration = (float)MusicEngineHelper.DurationOfBeat;
            
            // 色とテキストを変更
            ResetRingsColor(_newColor, _newColor);
        }
        
        #endregion
        
        #region チャージ攻撃（ボタンを離すタイミング）の演出

        /// <summary>
        /// 当たりエフェクトを強制再生
        /// TODO: 後に長押しノーツの判定のPlayerManager側が完成したら修正するかも
        /// </summary>
        protected override void OnPlayerSuccessForced(bool isPerfect)
        {
            // 他のすべてのTweenをキル
            for (int i = 0; i < _tweens.Length; i++)
            {
                _tweens[i]?.Kill();
            }
            
            if (isPerfect)
            {
                // パーフェクト判定の場合は収縮するリングのScaleを補正
                _ringImage.rectTransform.localScale = _contractionScale;
            }
            
            // 中央の画像を判定用の画像に変更
            HandleCenterImage(isPerfect);
            
            // 色変更前に白色のSpriteに変更
            ChangeRingsImage();
           
            var sequence = DOTween.Sequence()
                
                // 拡大
                .Append(_startPositionRing.rectTransform.DOScale(_centerRingsScale * 1.05f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_ringImage.rectTransform.DOScale(Vector3.one * _initialScale * 1.05f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_decorationImage.rectTransform.DOScale(_centerRingsScale * 1.05f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                
                // 色変更
                .Join(CreateColorChangeSequence(_newColor, _newTranslucentColor, _blinkDuration * 0.4f))
                
                // フェードアウト
                .Append(CreateFadeSequence(_fadeDuration))
                
                .OnComplete(End);

            _tweens[3] = sequence;
        }
        
        #endregion
        
        /// <summary>
        /// チャージ失敗時の失敗演出
        /// NOTE: ベースクラスで実装がある上のメソッドを使用すると、タイミングより後のミス以外処理がスキップされるため
        /// 長押しノーツの押し始めなどタイミングより前でミス処理が走った場合に対応していない
        /// </summary>
        private void PlayFailEffectCharging()
        {
            Unsubscribe();

            // Tweens配列をクリア
            if (_tweens != null)
            {
                for (int i = 0; i < _tweens.Length; i++)
                {
                    _tweens[i]?.Kill();
                }
            }
            
            // 中央のImageのスプライトとサイズをMissのものに変える
            SetMissImage();
            
            var failSequence = DOTween.Sequence();
            
            // 色変更とフェードアウト
            failSequence.Append(CreateColorChangeSequence(Color.darkGray, Color.darkGray, _fadeDuration));
            failSequence.Join(CreateFadeSequence(_fadeDuration));
            
            failSequence.OnComplete(End);
            
            _tweens[0] = failSequence;
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
            SetAllAlpha(0f);

			// 移動するオブジェクトの位置を変更
			_startPositionRing.transform.position = transform.position;

            _startPositionRing.sprite = _defaultEndRingSprite;
            _endPositionRing.sprite = _defaultEndRingSprite;
        }

        private void ChargeStart()
        {
            _startPositionRing.sprite = _commonSprite.Ring;
            base.ChangeRingsImage();
        }
        
        /// <summary>
        /// Perfect/Goodの色変更用にSpriteを白色のものに変更する
        /// </summary>
        protected override void ChangeRingsImage()
        {
            _startPositionRing.sprite = _commonSprite.Ring;
            _endPositionRing.sprite = _commonSprite.Ring;
            base.ChangeRingsImage();
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

        protected override void Subscribe()
        {
            // チャージ開始成功
            _player.OnPerfectCharging += OnPlayerCharge;
            _player.OnGoodCharging += OnPlayerCharge;
            
            // チャージ攻撃成功
            _player.OnPerfectChargeAttack += HandlePerfect; // Perfect成功
            _player.OnGoodChargeAttack += HandleGood; // Good成功
            
            // チャージ攻撃失敗
            //_player.OnMissedCharging += PlayFailEffectCharging;
            _player.OnMissChargeAttack += PlayFailEffectCharging;
        }
		
        protected override void Unsubscribe()
        {
            _player.OnPerfectCharging -= OnPlayerCharge;
            _player.OnGoodCharging -= OnPlayerCharge;
            _player.OnPerfectChargeAttack -= HandlePerfect;
            _player.OnGoodChargeAttack -= HandleGood;
            //_player.OnMissedCharging -= PlayFailEffectCharging;
            _player.OnMissChargeAttack -= PlayFailEffectCharging;
        }
        
        protected override void HandlePerfect() => OnPlayerSuccess(true);
        protected override void HandleGood() => OnPlayerSuccess(false);
    }
}
