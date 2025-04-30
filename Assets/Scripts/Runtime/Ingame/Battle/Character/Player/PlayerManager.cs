using System;
using SymphonyFrameWork.System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     プレイヤーのマネージャークラス
    /// </summary>
    public class PlayerManager : CharacterManagerB<CharacterData>, IDisposable
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

        private void OnAttack(InputAction.CallbackContext context)
        {
            Debug.Log($"{_data.Name} is attacking");
        }
    }
}
