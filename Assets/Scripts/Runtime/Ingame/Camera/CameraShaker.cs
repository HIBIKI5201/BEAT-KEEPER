using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.System;
using Unity.Cinemachine;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame
{
    /// <summary>
    /// カメラを揺らすためのスクリプト
    /// </summary>
    public class CameraShaker : MonoBehaviour
    {
        private CinemachineImpulseSource _impulseSource;
        private PlayerManager _playerManager;

        private void Awake()
        {
            _playerManager = GetComponent<PlayerManager>();
            _impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        private void OnEnable()
        {
            if (_playerManager != null)
            {
                _playerManager.OnHitAttack += HandleShake;
            }
        }

        private void OnDisable()
        {
            if (_playerManager != null)
            {
                _playerManager.OnHitAttack -= HandleShake;
            }
        }

        /// <summary>
        /// event Action登録用
        /// </summary>
        private void HandleShake(int _) => Shake();
        
        /// <summary>
        /// シェイクを発生させる
        /// </summary>
        private void Shake()
        {
            if (_impulseSource == null) return;
            _impulseSource.GenerateImpulse();
        }
    }
}
