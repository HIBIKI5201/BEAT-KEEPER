using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace BeatKeeper.Runtime.System
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

        #region Player
        public InputAction Attack => _attack;
        private InputAction _attack;

        public InputAction Interact => _interact;
        private InputAction _interact;

        public InputAction Avoid => _avoid;
        private InputAction _avoid;

        public InputAction Quit => _quit;
        private InputAction _quit;

        public InputAction AnyKey => _anyKey;
        private InputAction _anyKey;

        #endregion

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _uiInputModule = GetComponent<InputSystemUIInputModule>();

            if (_playerInput)
            {
                _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

                _attack = _playerInput.actions["Attack"];
                _interact = _playerInput.actions["Interact"];
                _avoid = _playerInput.actions["Avoid"];
                _quit = _playerInput.actions["Quit"];
                _anyKey = _playerInput.actions["AnyKey"];
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
