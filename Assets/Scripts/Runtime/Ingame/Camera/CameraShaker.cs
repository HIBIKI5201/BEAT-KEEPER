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

        private void Start()
        {
            _playerManager = GetComponent<PlayerManager>();
            _impulseSource = GetComponent<CinemachineImpulseSource>();
            
            _playerManager.OnHitAttack += HandleShake;
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
            _impulseSource.GenerateImpulse();
        }

        private void OnDestroy()
        {
            _playerManager.OnHitAttack -= HandleShake;
        }
    }
}
