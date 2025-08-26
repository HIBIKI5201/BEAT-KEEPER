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
                    _player.OnPerfectAttack += HandlePerfect;
                    _player.OnGoodAttack += HandleGood;
                    break;
            }
        }

        public override void End()
        {
            _player.OnPerfectAttack -= HandlePerfect;
            _player.OnGoodAttack -= HandleGood;

			base.End();
            
            // NOTE: InitializeComponents()より先に表示されてしまうのでここでも初期化を行う
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
        }

        #region チュートリアル用のメソッド

        /// <summary>
        /// Perfect判定時のエフェクト
        /// </summary>
        public void PlayPerfectEffect() => OnPlayerSuccessForced(true);

        /// <summary>
        /// Good判定時のエフェクト
        /// </summary>
        public void PlayGoodEffect() => OnPlayerSuccessForced(false);
        
        #endregion
        
        // Justタイミングは2拍後
        private const float CONTRACTION_SPEED = 2;
        // Justタイミングのあとの判定受付時間 // TODO: PlayerDataから値をとってくるようにする
        private const float RECEPTION_TIME = 0.45f;

        private void Start()
        {
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
            
            // スケールと色を初期化
            ResetRingsScale();
            ResetRingsColor(_defaultColor, _translucentDefaultColor);
        }
        
        /// <summary>
        /// リングの縮小
        /// </summary>
        private void StartContractionEffect()
        {
            // 一拍が何秒か、アニメーションのために値をキャッシュしておく
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
                .Append(_decorationImage.DOFade(_translucentDefaultColor.a * 1.5f, beatDuration * 0.5f).SetEase(Ease.OutSine))
                .Append(_decorationImage.DOFade(_translucentDefaultColor.a, beatDuration * 0.5f).SetEase(Ease.InSine))
                .SetLoops(-1, LoopType.Restart);
            
            _tweens[1] = blurPulseSequence;
        }
        
        protected override void HandlePerfect() => OnPlayerSuccess(true);
        protected override void HandleGood() => OnPlayerSuccess(false);
    }
}
