using BeatKeeper.Runtime.Ingame.Character;
using Unity.Cinemachine;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame
{
    /// <summary>
    /// カメラを揺らすためのスクリプト
    /// </summary>
    public class CameraShaker : MonoBehaviour
    {
        [SerializeField] private CinemachineImpulseSource _impulseSource;
        [SerializeField] private PlayerManager _playerManager;

        private void Start()
        {
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
            if(!_impulseSource) _impulseSource = GetComponent<CinemachineImpulseSource>();
            _impulseSource.GenerateImpulse();
        }

        private void OnDestroy()
        {
            _playerManager.OnHitAttack -= HandleShake;
        }
    }
}
