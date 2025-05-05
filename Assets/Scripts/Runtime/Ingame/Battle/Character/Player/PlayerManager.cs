using System;
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

        private IAttackable _target;
        
        public ComboSystem ComboSystem => _comboSystem;
        private ComboSystem _comboSystem;

        public event Action OnResonanceHit;
        
        protected override void Awake()
        {
            base.Awake();
            
            _comboSystem = new ComboSystem(_data);
            
            tag = TagsEnum.Player.ToString();
            
            #if UNITY_EDITOR
            OnResonanceHit += () => Debug.Log("Resonance Hit");
            #endif
        }

        private void Start()
        {
            _inputBuffer = ServiceLocator.GetInstance<InputBuffer>();
            _musicEngine = ServiceLocator.GetInstance<MusicEngineHelper>();

            if (_inputBuffer)
            {
                _inputBuffer.Attack.started += OnAttack;
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
            _inputBuffer.Attack.started -= OnAttack;
        }

        /// <summary>
        ///     コンボ攻撃を行う
        /// </summary>
        /// <param name="context"></param>
        private void OnAttack(InputAction.CallbackContext context)
        {
            Debug.Log($"{_data.Name} is attacking");

            //リズム共鳴が成功したか
            bool isResonanceHit = _musicEngine.IsTimingWithinAcceptableRange(_data.ResonanceRange);
            if (isResonanceHit) OnResonanceHit?.Invoke();

            
            //コンボに応じたダメージ
            var power = (_comboSystem.ComboCount % 3) switch
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
        }

        /// <summary>
        ///     溜め攻撃を行う
        /// </summary>
        /// <param name="context"></param>
        private void OnChargeAttack(InputAction.CallbackContext context)
        {
            Debug.Log($"{_data.Name} is charging");
        }

        public override void HitAttack(float damage)
        {
            base.HitAttack(damage);
            
            _comboSystem.ComboReset();
        }
        
        public void SetTarget(IAttackable target) => _target = target;
    }
}
