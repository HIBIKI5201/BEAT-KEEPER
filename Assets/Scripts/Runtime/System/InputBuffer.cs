using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace BeatKeeper
{
    /// <summary>
    ///     入力を受け取るバッファー
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class InputBuffer : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private InputSystemUIInputModule _uiInputModule;
        public InputSystemUIInputModule uiInputModule => _uiInputModule;

        # region Player
        
        public InputAction Move => _move;
        private InputAction _move;
        
        public InputAction Look => _look;
        private InputAction _look;
        
        public InputAction Attack => _attack;
        private InputAction _attack;
        
        public InputAction Interact => _interact;
        private InputAction _interact;

        public InputAction Avoid => _avoid;
        private InputAction _avoid;
        
        public InputAction Skill => _skill;
        private InputAction _skill;
        
        public InputAction Special => _special;
        private InputAction _special;
                
        public InputAction Finishier => _finisher;
        private InputAction _finisher;

        public InputAction Quit => _quit;
        private InputAction _quit;
        
        #endregion

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _uiInputModule = GetComponent<InputSystemUIInputModule>();
            
            if (_playerInput)
            {
                _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

                _move = _playerInput.actions["Move"];
                _look = _playerInput.actions["Look"];
                _attack = _playerInput.actions["Attack"];
                _interact = _playerInput.actions["Interact"];
                _avoid = _playerInput.actions["Avoid"];
                _skill = _playerInput.actions["Skill"];
                _special = _playerInput.actions["Special"];
                _finisher = _playerInput.actions["Finisher"];
                _quit = _playerInput.actions["Quit"];
            }
            else
            {
                Debug.LogWarning("PlayerInput is null");
            }

            if (_quit != null)
            {
                _quit.started += n => Application.Quit();
            }
        }

        /// <summary>
        ///     アクションマップを切り替える
        /// </summary>
        /// <param name="type"></param>
        public void ChangeActionMap(ActionMapType type) => _playerInput.SwitchCurrentActionMap(type.ToString());

        public enum ActionMapType
        {
            Player,
            UI
        }
    }
}
