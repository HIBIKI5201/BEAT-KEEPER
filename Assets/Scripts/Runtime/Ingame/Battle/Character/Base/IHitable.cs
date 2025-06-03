using System;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public interface IHitable
    {
        public void HitAttack(float damage);

        public Action OnDeath { get; set; }
        public Action<int> OnHitAttack { get; set; }
    }
}
