using System;
using System.Threading.Tasks;
using BeatKeeper.Runtime.Ingame.Battle;
using BeatKeeper.Runtime.Ingame.Stsge;
using Cysharp.Threading.Tasks;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.InputSystem;
using R3;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     プレイヤーのマネージャークラス
    /// </summary>
    public class PlayerManager : CharacterManagerB<PlayerData>, IDisposable
    {
        private InputBuffer _inputBuffer;
        private MusicEngineHelper _musicEngine;
        private PlayerAnimeManager _animeManager;

        private bool _isBattle;

        private IEnemy _target;

        public ComboSystem ComboSystem => _comboSystem;
        private ComboSystem _comboSystem;

        public SpecialSystem SpecialSystem => _specialSystem;
        private SpecialSystem _specialSystem;

        public event Action OnComboAttack;
        public event Action OnResonanceAttack;
        public event Action OnNonResonanceAttack;
        public event Action OnJustAvoid;

        #region モック用の機能

        [SerializeField] private ParticleSystem _particleSystem;

        #endregion

        // NOTE: フローゾーンシステムを作成してみました。設計に合わせて修正してください
        public FlowZoneSystem FlowZoneSystem => _flowZoneSystem;
        private FlowZoneSystem _flowZoneSystem;

        protected override void Awake()
        {
            base.Awake();
        }

        private async void OnEnable()
        {
            _comboSystem = new ComboSystem(_data);
            _specialSystem = new SpecialSystem();
            _flowZoneSystem =
                new FlowZoneSystem(await ServiceLocator.GetInstanceAsync<MusicEngineHelper>());
            
            if (TryGetComponent(out Animator animator))
                _animeManager = new(animator);
            else Debug.LogWarning("Character animator component not found");

            OnComboAttack += _comboSystem.Attack;
            OnComboAttack += _particleSystem.Play;
            OnResonanceAttack += () => _specialSystem.AddSpecialEnergy(0.05f);
            OnJustAvoid += _flowZoneSystem.SuccessResonance;

            OnHitAttack += _comboSystem.ComboReset;
        }

        private void Start()
        {
            _inputBuffer = ServiceLocator.GetInstance<InputBuffer>();
            _musicEngine = ServiceLocator.GetInstance<MusicEngineHelper>();

            if (_inputBuffer) //入力を購買する
            {
                _inputBuffer.Attack.started += OnAttack;
                _inputBuffer.Special.started += OnSpecial;
                _inputBuffer.Finishier.started += OnFinisher;
                _inputBuffer.Avoid.started += OnAvoid;
            }
            else
            {
                Debug.LogWarning("Input buffer is null");
            }

            if (!_musicEngine)
                Debug.LogWarning("Music engine is null");

            var phaseManager = ServiceLocator.GetInstance<PhaseManager>();
            if (phaseManager)
            {
                phaseManager.CurrentPhaseProp
                    .Subscribe(OnPhaseChanged)
                    .AddTo(destroyCancellationToken);
            }
            else
            {
                Debug.LogWarning("Phase manager is null");
            }
        }

        private void Update()
        {
            _comboSystem.Update();
        }

        public void Dispose()
        {
            if (_inputBuffer) //入力の購買を終わる
            {
                _inputBuffer.Attack.started -= OnAttack;
                _inputBuffer.Special.started -= OnSpecial;
                _inputBuffer.Finishier.started -= OnFinisher;
                _inputBuffer.Avoid.started -= OnAvoid;
            }
        }

        public override void HitAttack(float damage)
        {
            base.HitAttack(damage);
            OnHitAttack?.Invoke(Mathf.FloorToInt(damage)); ////
        }

        public void SetTarget(IEnemy target) => _target = target;

        private void OnPhaseChanged(PhaseEnum phase)
        {
            _isBattle = phase == PhaseEnum.Battle;

            //ターゲットを探す
            var stage = ServiceLocator.GetInstance<BattleSceneManager>();
            _target = stage.EnemyAdmin.FindClosestEnemy(transform.position);
        }

        /// <summary>
        ///     コンボ攻撃を行う
        /// </summary>
        /// <param name="context"></param>
        private void OnAttack(InputAction.CallbackContext context)
        {
            if (!_isBattle) return;
            if (_target == null) return;

            Debug.Log($"{_data.Name} is attacking");
            OnComboAttack?.Invoke();

            //リズム共鳴が成功したか
            bool isResonanceHit = _musicEngine.IsTimingWithinAcceptableRange(_data.ResonanceRange);
            if (isResonanceHit)
            {
                OnResonanceAttack?.Invoke();
            }
            else
            {
                OnNonResonanceAttack?.Invoke();
            }

            //コンボに応じたダメージ
            var power = (_comboSystem.ComboCount.CurrentValue % 3) switch
            {
                0 => _data.FirstAttackPower,
                1 => _data.SecondAttackPower,
                2 => _data.ThirdAttackPower,

                _ => _data.FirstAttackPower
            };
            if (isResonanceHit)
                power *= _data.ResonanceCriticalDamage;

            _target.HitAttack(power);
        }

        /// <summary>
        ///     溜め攻撃を行う
        /// </summary>
        /// <param name="context"></param>
        private void OnChargeAttack(InputAction.CallbackContext context)
        {
            if (!_isBattle) return;

            Debug.Log($"{_data.Name} is charge attacking");
        }

        /// <summary>
        ///     必殺技
        /// </summary>
        /// <param name="context"></param>
        private void OnSpecial(InputAction.CallbackContext context)
        {
            if (!_isBattle) return;
            if (_target == null) return;

            if (1 <= _specialSystem.SpecialEnergy.CurrentValue)
            {
                _specialSystem.ResetSpecialEnergy();
                _target.HitAttack(2000);
            }
        }

        /// <summary>
        ///     フィニッシャー
        /// </summary>
        /// <param name="context"></param>
        private void OnFinisher(InputAction.CallbackContext context)
        {
            if (!_isBattle) return;

            Debug.Log($"{_data.Name} is attacking");
        }

        /// <summary>
        ///     回避
        /// </summary>
        /// <param name="context"></param>
        private void OnAvoid(InputAction.CallbackContext context)
        {
            if (!_isBattle) return;
            Debug.Log($"{_data.Name} is avoiding");

            var timing = _musicEngine.GetCurrentTiming() switch
            {
                var data => (data.Bar * 4 + data.Beat) % 32 //節と拍を足した値
            };

            //nターン後までに攻撃があるかどうか
            bool willAttack = false;
            for (int i = 0; i < 3; i++)
            {
                willAttack |= _target.EnemyData.Beat[(timing + i) % 32];
                if (willAttack) break; //あったら終了
            }

            if (willAttack)
            {
                Debug.Log($"Enemy will be attack player");
                OnJustAvoid?.Invoke();
            }
        }

        [ContextMenu(nameof(AddSpecialEnergy))]
        private void AddSpecialEnergy() => _specialSystem.AddSpecialEnergy(1);
    }
}