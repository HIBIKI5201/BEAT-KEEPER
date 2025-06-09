using BeatKeeper.Runtime.Ingame.Battle;
using System;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public interface IHitable
    {
        public void HitAttack(AttackData damage);

        public Action<int> OnHitAttack { get; set; }
    }
}
