using BeatKeeper.Runtime.Ingame.System;
using BeatKeeper.Runtime.System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BeatKeeper.Runtime.Ingame.UI
{
    /// <summary>
    ///     敵の攻撃警告・回避UI
    /// </summary>
    public class EnemyIndicator : RingIndicatorBase
    {
        public override int EffectLength => 3;

        public override void End()
        {
            base.End();

            //敵攻撃はノックバックを与えるので確認
            _chartRingManager.CheckAllRingIndicatorRemainTime();
        }
        
        #region チュートリアル用のメソッド

        public void OnPlayerAvoidSuccess(bool isPerfect)
        {
            OnPlayerSuccessForced(isPerfect);
        }
        
        #endregion

        protected override void Subscribe()
        {
            _player.OnPerfectAvoid += HandlePerfect;
            _player.OnGoodAvoid += HandleGood;
            _player.OnFailedAvoid += PlayFailEffect;
        }
		
        protected override void Unsubscribe()
        {
            _player.OnPerfectAvoid -= HandlePerfect;
            _player.OnGoodAvoid -= HandleGood;
            _player.OnFailedAvoid -= PlayFailEffect;
        }
        
		protected override void HandlePerfect() => OnPlayerSuccess(true);
        protected override void HandleGood() => OnPlayerSuccess(false);
    }
}
