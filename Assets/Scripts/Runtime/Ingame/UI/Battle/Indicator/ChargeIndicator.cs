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
        
        #region チュートリアル用のメソッド
        
        /// <summary>
        /// チャージ演出
        /// </summary>
        public void OnPlayerChargeTutorial() => OnPlayerChargeForced();

        /// <summary>
        /// 長押し成功演出
        /// </summary>
        public void OnPlayerAttackSuccessTutorial() => OnPlayerAttackSuccessForced();
        
        #endregion
        
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
            SetAllAlpha(1f);
            
            _tweens = new Tween[5];
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

            // アーチのパスを作成
            Vector3 startPos = _startPositionRing.transform.position;
            Vector3 endPos = _endPositionRing.transform.position;
    
            // アーチの頂点を計算（中間点から上方向にオフセット）
            Vector3 midPoint = (startPos + endPos) * 0.5f;
            Vector3 archTop = midPoint + Vector3.up * Vector3.Distance(startPos, endPos) * 0.15f; // 高さを計算
    
            Vector3[] pathPoints = { startPos, archTop, endPos };
            
            var sequence = DOTween.Sequence()
                
                // 色変更（チャージ開始時）
                .Append(_ringImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 縮小するリング
                .Join(_decorationImage.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 縮小するリングの発光部分
                .Join(_startPositionRing.DOColor(_chargeColor, totalDuration * 0.1f).SetEase(Ease.OutFlash)) // 自身
        
                // メインのリング移動アニメーション（外側リングから内側リングへ）
                .Append(_ringImage.rectTransform.DOScale(_centerRingsScale, totalDuration * 0.9f).SetEase(Ease.OutQuart))
        
                // アーチの動きを表現
                .Join(_startPositionRing.rectTransform.DOPath(pathPoints, totalDuration * 0.9f, PathType.CatmullRom).SetEase(Ease.Linear))

                .OnComplete(OnChargeComplete);
            
            _tweens[0] = sequence;
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

            Unsubscribe();

            OnPlayerAttackSuccessForced();
        }

        /// <summary>
        /// 当たりエフェクトを強制再生
        /// TODO: 後に長押しノーツの判定のPlayerManager側が完成したら修正するかも
        /// </summary>
        private void OnPlayerAttackSuccessForced()
        {
            // 他のすべてのTweenをキル
            for (int i = 0; i < _tweens.Length; i++)
            {
                _tweens[i]?.Kill();
            }
            
            // 中央の画像を判定用の画像に変更
            // TODO: 判定に合わせて引数に渡す変数を変更する
            HandleCenterImage(true);
            
            // 色変更前に白色のSpriteに変更
            ChangeRingsImage();
           
            var sequence = DOTween.Sequence()
                
                // 拡大
                .Append(_startPositionRing.rectTransform.DOScale(_centerRingsScale * 1.05f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_ringImage.rectTransform.DOScale(Vector3.one * _initialScale * 1.05f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                .Join(_decorationImage.rectTransform.DOScale(_centerRingsScale * 1.05f, _blinkDuration * 0.4f).SetEase(Ease.OutBack))
                
                // 色変更
                .Join(CreateColorChangeSequence(_newColor, _newTranslucentColor, _blinkDuration * 0.4f))
                
                // フェードアウト NOTE: 他のノーツと違いここの連結をJoinとしている
                .Append(CreateFadeSequence(_fadeDuration))
                
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

        protected override void Subscribe()
        {
            _player.OnStartChargeAttack += OnPlayerCharge; // チャージ開始
            _player.OnChargeAttack += OnPlayerAttackSuccess; // チャージ完了したあとに攻撃
            _player.OnMissChargeAttack += PlayFailEffect; // チャージ完了前に攻撃（=チャージ攻撃失敗）
        }
		
        protected override void Unsubscribe()
        {
            _player.OnStartChargeAttack -= OnPlayerCharge;
            _player.OnChargeAttack -= OnPlayerAttackSuccess;
            _player.OnMissChargeAttack -= PlayFailEffect;
        }
    }
}
