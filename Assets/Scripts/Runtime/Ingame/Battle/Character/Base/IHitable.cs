using BeatKeeper.Runtime.Ingame.Battle;
using System;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///   攻撃を受けることができるインターフェース
    /// </summary>
    public interface IHitable
    {
        /// <summary>
        /// /     攻撃を受けた時の処理
        /// </summary>
        /// <param name="damage"></param>
        public void HitAttack(AttackData damage);

        public Action<int> OnHitAttack { get; set; }
    }
}
