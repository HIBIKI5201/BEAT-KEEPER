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

        private IAttackable _target;
        
        public ComboSystem ComboSystem => _comboSystem;
        private readonly ComboSystem _comboSystem = new();
        
        protected override void Awake()
        {
            base.Awake();
            
            tag = TagsEnum.Player.ToString();
        }

        private void Start()
        {
            _inputBuffer = ServiceLocator.GetInstance<InputBuffer>();

            _inputBuffer.Attack.started += OnAttack;
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

            //コンボに応じたダメージ
            var power = (_comboSystem.ComboCount % 3) switch
            {
                0 => _data.FirstAttackPower,
                1 => _data.SecondAttackPower,
                2 => _data.ThirdAttackPower,

                _ => _data.FirstAttackPower
            };
            
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
