using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;

namespace BeatKeeper
{
    public class TestEnemyManager : CharacterManagerB<EnemyData>
    {
        private bool _isKnockback;
        
        protected override void Awake()
        {
            base.Awake();
        }

        public override async void HitAttack(float damage)
        {
            base.HitAttack(damage);
            
            //ノックバック
            _isKnockback = true;
            await Awaitable.WaitForSecondsAsync(1, destroyCancellationToken);
            _isKnockback = false;
        }
    }
}
