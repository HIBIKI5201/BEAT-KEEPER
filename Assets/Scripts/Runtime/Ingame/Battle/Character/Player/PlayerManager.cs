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

        private int _comboCount;
        
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
            
            
        }

        /// <summary>
        ///     溜め攻撃を行う
        /// </summary>
        /// <param name="context"></param>
        private void OnChargeAttack(InputAction.CallbackContext context)
        {
            Debug.Log($"{_data.Name} is charging");
        }
    }
}
