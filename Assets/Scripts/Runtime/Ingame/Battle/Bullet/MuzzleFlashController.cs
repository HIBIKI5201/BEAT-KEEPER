using System;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame
{
    /// <summary>
    /// 攻撃時のマズルフラッシュエフェクトを管理するクラス
    /// </summary>
    [Serializable]
    public class MuzzleFlashController
    {
        [SerializeField] private Transform _muzzle; // 銃口の位置
        [SerializeField] private ParticleSystem _muzzleFlash; // マズルフラッシュのエフェクト

        public MuzzleFlashController(Transform muzzle, ParticleSystem muzzleFlash)
        {
            _muzzle = muzzle;
            _muzzleFlash = muzzleFlash;
        }
        
        /// <summary>
        /// 弾を発射する
        /// </summary>
        public void Fire()
        {
            if (_muzzleFlash != null)
            {
                // エフェクトが設定されていたらマズルフラッシュを再生
                _muzzleFlash.transform.position = _muzzle.position; // 座標を銃口に合わせる
                _muzzleFlash.Play();
            }
        }
    }
}
