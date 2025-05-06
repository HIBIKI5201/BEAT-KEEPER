using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;

namespace BeatKeeper
{
    public class EnemyManager : CharacterManagerB<EnemyData>
    {
        private EnemyAnimeManager _animeManager;
        
        private bool _isKnockback;
        
        protected override void Awake()
        {
            base.Awake();
            
            var animator = GetComponent<Animator>();
            if (animator)
            {
                _animeManager = new (animator);
            }
            else
            {
                Debug.LogWarning($"{_data.name} has no Animator");
            }
        }

        public override async void HitAttack(float damage)
        {
            base.HitAttack(damage);
            
            //ノックバック
            _isKnockback = true;
            _animeManager.KnockBack();
            await Awaitable.WaitForSecondsAsync(1, destroyCancellationToken);
            _isKnockback = false;
        }
    }
}
