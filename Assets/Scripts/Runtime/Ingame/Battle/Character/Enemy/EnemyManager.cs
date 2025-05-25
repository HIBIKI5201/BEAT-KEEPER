using System;
using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public class EnemyManager : CharacterManagerB<EnemyData>, IEnemy, IDisposable
    {
        EnemyData IEnemy.EnemyData => _data;
        
        private MusicEngineHelper _musicEngine;
        private ScoreManager _scoreManager;
        private EnemyAnimeManager _animeManager;
        private CharacterHealthSystem _healthSystem;

        private IHitable _target;
        
        private bool _isKnockback;

        public event Action OnNormalAttack; 
        
        #region モック用の機能
        
        [SerializeField] private ParticleSystem _particleSystem;
        
        #endregion

        private void OnEnable()
        {
            if (TryGetComponent(out Animator animator))
            {
                _animeManager = new (animator);
            }
            else
            {
                Debug.LogWarning($"{_data.name} has no Animator");
            }

            _healthSystem = new(_data.MaxHealth);
            
            OnHitAttack += _animeManager.KnockBack;
        }

        private void Start()
        {
            _musicEngine = ServiceLocator.GetInstance<MusicEngineHelper>();
            _scoreManager = ServiceLocator.GetInstance<ScoreManager>();
            _target = ServiceLocator.GetInstance<PlayerManager>();

            if (_musicEngine)
            {
                _musicEngine.OnJustChangedBeat += OnAttack;
            }
            else
            {
                Debug.LogWarning($"{_data.name} has no music engine");
            }
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
            
            OnHitAttack?.Invoke(Mathf.FloorToInt(damage)); ////
            _scoreManager.AddScore(Mathf.FloorToInt(damage)); // スコアを加算。小数点以下は切り捨てる
            
            //ノックバック
            _isKnockback = true;
            await Awaitable.WaitForSecondsAsync(_data.NockbackTime, destroyCancellationToken);
            _isKnockback = false;
        }

        private void OnAttack()
        {
            if (!_musicEngine) return;
            if (_isKnockback) return; //ノックバック中は攻撃しない

            OnNormalAttack?.Invoke();

            var timing = _musicEngine.GetCurrentTiming();

            if (_data.IsAttack(timing.Bar * 4 + timing.Beat))
            {
                Debug.Log($"{_data.name} {_data.Chart[(timing.Bar * 4 + timing.Beat) % 32]} attack\ntiming : {timing}");
                
                _target.HitAttack(1);
                _particleSystem?.Play();
            }
        }
    }
}
