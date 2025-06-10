using BeatKeeper.Runtime.Ingame.Battle;
using Cysharp.Threading.Tasks;
using R3;
using SymphonyFrameWork.System;
using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    public class EnemyManager : CharacterManagerB<EnemyData>, IEnemy, IDisposable
    {
        EnemyData IEnemy.EnemyData => _data;

        public event Action OnFinisherable;
        public event Action OnNormalAttack;

        private MusicEngineHelper _musicEngine;

        private IHitable _target;

        private bool _canFinisher;
        private bool _isKnockback;

        private EnemyAnimeManager _animeManager;
        private CharacterHealthSystem _healthSystem;
        public CharacterHealthSystem HealthSystem => _healthSystem;

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

            if (!_musicEngine)
            {
                Debug.LogWarning($"{_data.name} has no music engine");
            }

            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            phaseManager.CurrentPhaseProp
                .Subscribe(OnPhaseChange)
                .AddTo(destroyCancellationToken);
        }

        /// <summary>
        ///     入力の登録を行う
        /// </summary>
        public void InputRegister()
        {
            if (_musicEngine)
            {
                _musicEngine.OnJustChangedBeat += OnAttack;
            }
        }

        /// <summary>
        ///     入力の登録を解除する
        /// </summary>
        public void InputUnregister()
        {
            if (_musicEngine)
            {
                _musicEngine.OnJustChangedBeat -= OnAttack;
            }
        }

        public void Dispose()
        {
            InputUnregister();
        }

        public override void HitAttack(AttackData data)
        {
            base.HitAttack(data);

            _healthSystem?.HealthChange(-data.Damage);

            OnHitAttack?.Invoke(Mathf.FloorToInt(data.Damage));

            //フィニッシャー可能範囲の処理
            if (_healthSystem.Health / _healthSystem.MaxHealth 
                <= _data.FinisherThreshold / 100) //フィニッシャー可能割合の判定
            {
                if (!_canFinisher) //初回時のみ
                {
                    Debug.Log("Finisherable event triggered for " + _data.name);

                    InputUnregister(); // 入力の登録を解除
                    _canFinisher = true;
                    OnFinisherable?.Invoke();
                }
            }

            if (data.IsNockback) //ノックバックする
            {
                Nockback();
            }
        }

        /// <summary>
        ///     フェーズが変わったときの処理
        /// </summary>
        private void OnPhaseChange(PhaseEnum phase)
        {
            if (phase == PhaseEnum.Battle) //戦闘フェーズが始まったら動き始める
            {
                InputRegister();
            }
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

        private async void Nockback()
        {
            _isKnockback = true;
            _animeManager?.KnockBack();
            await Awaitable.WaitForSecondsAsync(_data.NockbackTime, destroyCancellationToken);
            _isKnockback = false;
        }
    }
}
