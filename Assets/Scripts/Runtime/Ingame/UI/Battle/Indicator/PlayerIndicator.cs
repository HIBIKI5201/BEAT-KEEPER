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

		protected override void Subscribe()
		{
			_player.OnPerfectAttack += HandlePerfect;
            _player.OnGoodAttack += HandleGood;
		}
		
		protected override void Unsubscribe()
		{
			_player.OnPerfectAttack -= HandlePerfect;
            _player.OnGoodAttack -= HandleGood;
		}

        protected override void HandlePerfect() => OnPlayerSuccess(true);
        protected override void HandleGood() => OnPlayerSuccess(false);
    }
}
