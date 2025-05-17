using System;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public interface IHitable
    {
        public void HitAttack(float damage);
        
        public Action OnHitAttack { get; set; }
    }
}
