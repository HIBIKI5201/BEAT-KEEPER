using System;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper
{
    public class EnemyManager : CharacterManagerB<EnemyData>, IDisposable
    {
        private MusicEngineHelper _musicEngine;
        private EnemyAnimeManager _animeManager;

        private IAttackable _target;
        
        private bool _isKnockback;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (TryGetComponent(out Animator animator))
            {
                _animeManager = new (animator);
            }
            else
            {
                Debug.LogWarning($"{_data.name} has no Animator");
            }
        }

        private void Start()
        {
            _musicEngine = ServiceLocator.GetInstance<MusicEngineHelper>();

            if (_musicEngine)
            {
                _musicEngine.OnJustChangedBeat += OnAttack;
            }
            else
            {
                Debug.LogWarning($"{_data.name} has no music engine");
            }
            
            Music.Play("Music");
        }

        public void Dispose()
        {
            if (_musicEngine)
            {
                _musicEngine.OnJustChangedBeat -= OnAttack;
            }
        }

        public override async void HitAttack(float damage)
        {
            base.HitAttack(damage);
            
            //ノックバック
            _isKnockback = true;
            _animeManager.KnockBack();
            await Awaitable.WaitForSecondsAsync(_data.NockbackTime, destroyCancellationToken);
            _isKnockback = false;
        }

        private void OnAttack()
        {
            if (_musicEngine) return;
            
            if (_isKnockback) return; //ノックバック中は攻撃しない
            
            var timing = _musicEngine.GetCurrentTiming() switch
            {
                var data => (data.Bar * 4 + data.Beat) % 32 //節と拍を足した値
            };

            if (_data.Beat[timing])
            {
                Debug.Log($"{_data.name} attack {timing}");
            }
        }
    }
}
