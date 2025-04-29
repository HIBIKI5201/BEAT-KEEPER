using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BeatKeeper
{
    /// <summary>
    ///     入力を受け取るバッファー
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class InputBuffer : MonoBehaviour
    {
        private PlayerInput _playerInput;

        public InputAction Move => _move;
        private InputAction _move;
        
        public InputAction Attack => _attack;
        private InputAction _attack;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            if (_playerInput)
            {
                _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

                _move = _playerInput.actions["Move"];
                _attack = _playerInput.actions["Attack"];
            }
            else
            {
                Debug.LogWarning("PlayerInput is null");
            }
        }
    }
}
