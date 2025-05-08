using System;
using BeatKeeper.Runtime.Ingame.Battle.Character;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.InputSystem;

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
        
        private IEnemy _target;
        
        public ComboSystem ComboSystem => _comboSystem;
        private ComboSystem _comboSystem;
        
        public SpecialSystem SpecialSystem => _specialSystem;
        private SpecialSystem _specialSystem;
        
        public event Action OnResonanceHit;
        
        protected override void Awake()
        {
            base.Awake();
            
            _comboSystem = new ComboSystem(_data);

            if (TryGetComponent(out Animator animator))
            {
                _animeManager = new (animator);
            }
            else
            {
                Debug.LogWarning("Character animator component not found");
            }
        }

        private void Start()
        {
            _inputBuffer = ServiceLocator.GetInstance<InputBuffer>();
            _musicEngine = ServiceLocator.GetInstance<MusicEngineHelper>();

            if (_inputBuffer)
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
        }

        private void Update()
        {
             _comboSystem.Update();
        }

        public void Dispose()
        {
            if (_inputBuffer)
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
            
            _comboSystem.ComboReset();
        }
        
        public void SetTarget(IEnemy target) => _target = target;

        /// <summary>
        ///     コンボ攻撃を行う
        /// </summary>
        /// <param name="context"></param>
        private void OnAttack(InputAction.CallbackContext context)
        {
            if (_target == null)
            {
                Debug.LogWarning("Target is null");
                return;
            }
            
            Debug.Log($"{_data.Name} is attacking");

            //リズム共鳴が成功したか
            bool isResonanceHit = _musicEngine.IsTimingWithinAcceptableRange(_data.ResonanceRange);
            if (isResonanceHit) OnResonanceHit?.Invoke();
            
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
            
            _comboSystem.Attack();
            
            //スペシャルエネルギーを5%増加
            if (isResonanceHit)
                _specialSystem.AddSpecialEnergy(0.05f);
        }

        /// <summary>
        ///     溜め攻撃を行う
        /// </summary>
        /// <param name="context"></param>
        private void OnChargeAttack(InputAction.CallbackContext context)
        {
            Debug.Log($"{_data.Name} is charge attacking");
        }

        /// <summary>
        ///     必殺技
        /// </summary>
        /// <param name="context"></param>
        private void OnSpecial(InputAction.CallbackContext context)
        {
            if (_target == null)
            {
                Debug.LogWarning("Target is null");
                return;
            }
            
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
            Debug.Log($"{_data.Name} is attacking");
        }

        /// <summary>
        ///     回避
        /// </summary>
        /// <param name="context"></param>
        private void OnAvoid(InputAction.CallbackContext context)
        {
            Debug.Log($"{_data.Name} is avoiding");
        }
        
        [ContextMenu(nameof(AddSpecialEnergy))]
        private void AddSpecialEnergy() => _specialSystem.AddSpecialEnergy(1);
    }
}
