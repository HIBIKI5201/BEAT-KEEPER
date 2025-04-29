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

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            if (_playerInput)
            {
                _playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;
            }
            else
            {
                Debug.LogWarning("PlayerInput is null");
            }
        }
    }
}
