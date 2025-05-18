using BeatKeeper.Runtime.Ingame.Character;
using SymphonyFrameWork.System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame
{
    /// <summary>
    /// プレイヤーのマズルフラッシュエフェクトの再生タイミングを管理するクラス
    /// </summary>
    public class PlayerMuzzleFlashHandler : MonoBehaviour
    {
        [SerializeField] private MuzzleFlashController _muzzleFlash = new MuzzleFlashController();
        private PlayerManager _playerManager;
        
        private void Start()
        {
            _playerManager = ServiceLocator.GetInstance<PlayerManager>();
            
            // プレイヤーが攻撃したタイミングでマズルフラッシュのエフェクトが再生されるようにする
            _playerManager.OnComboAttack += _muzzleFlash.Fire;
        }

        private void OnDestroy()
        {
            _playerManager.OnComboAttack -= _muzzleFlash.Fire;
        }
    }
}
