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

        protected override void HandlePerfect() => OnPlayerSuccess(true);
        protected override void HandleGood() => OnPlayerSuccess(false);
    }
}
