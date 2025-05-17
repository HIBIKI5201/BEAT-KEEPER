using UnityEngine;

namespace BeatKeeper.Runtime.Ingame
{
    /// <summary>
    /// 被弾エフェクトを管理するクラス
    /// </summary>
    public class HitEffectController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _hitEffect; // 被弾時のエフェクト
        
    }
}
