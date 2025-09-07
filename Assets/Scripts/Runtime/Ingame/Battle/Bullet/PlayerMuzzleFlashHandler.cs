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
        
        private async void Start()
        {
            _playerManager = await ServiceLocator.GetInstanceAsync<PlayerManager>();

            if (_playerManager != null)
            {
                // プレイヤーが攻撃したタイミングでマズルフラッシュのエフェクトが再生されるようにする
                _playerManager.OnShootComboAttack += _muzzleFlash.Fire;
            }
            else
            {
                Debug.LogError($"{typeof(PlayerMuzzleFlashHandler)} PlayerManagerが取得できませんでした。マズルフラッシュエフェクトが再生できません");
            }
        }

        private void OnDestroy()
        {
            if (_playerManager != null)
            {
                _playerManager.OnShootComboAttack -= _muzzleFlash.Fire;
            }
        }
    }
}
