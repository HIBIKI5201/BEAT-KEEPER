using BeatKeeper.Runtime.Ingame.Battle;
using SymphonyFrameWork.System;
using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public class EnemyManager : CharacterManagerB<EnemyData>, IEnemy, IDisposable
    {
        EnemyData IEnemy.EnemyData => _data;

        private MusicEngineHelper _musicEngine;
        private EnemyAnimeManager _animeManager;
        public CharacterHealthSystem HealthSystem => _healthSystem;
        private CharacterHealthSystem _healthSystem;

        private IHitable _target;

        private bool _canFinisher;
        private bool _isKnockback;

        public event Action OnFinisherable;
        public event Action OnNormalAttack;

        #region モック用の機能

        [SerializeField, Obsolete("モック用")] private ParticleSystem _particleSystem;

        #endregion

        private void OnEnable()
        {
            if (TryGetComponent(out Animator animator))
            {
                _animeManager = new(animator);
            }
            else
            {
                Debug.LogWarning($"{_data.name} has no Animator");
            }

            _healthSystem = new(_data);
        }

        private void Start()
        {
            _musicEngine = ServiceLocator.GetInstance<MusicEngineHelper>();
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

        public override async void HitAttack(AttackData data)
        {
            base.HitAttack(data);

            _healthSystem?.HealthChange(-data.Damage);
            _animeManager?.KnockBack();

            OnHitAttack?.Invoke(Mathf.FloorToInt(data.Damage));

            if (_healthSystem.Health / _healthSystem.MaxHealth <= _data.FinisherThreshold / 100)
            {
                if (!_canFinisher)
                {
                    Debug.Log("Finisherable event triggered for " + _data.name);

                    _canFinisher = true;
                    OnFinisherable?.Invoke();
                }
            }

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

            var timing = MusicEngineHelper.GetCurrentTiming();

            if (_data.ChartData.IsEnemyAttack(timing.Bar * 4 + timing.Beat))
            {
                Debug.Log($"{_data.name} {_data.ChartData.Chart[(timing.Bar * 4 + timing.Beat) % 32]} attack\ntiming : {timing}");

                _target.HitAttack(new AttackData(1));
                _particleSystem?.Play();
            }
        }
    }
}
