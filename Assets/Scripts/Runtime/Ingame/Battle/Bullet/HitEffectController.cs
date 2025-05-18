using BeatKeeper.Runtime.Ingame.Character;
using UnityEngine;

namespace BeatKeeper.Runtime.Ingame
{
    /// <summary>
    /// 被弾エフェクトを管理するクラス
    /// </summary>
    public class HitEffectController : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _hitEffect; // 被弾時のエフェクト
        private IHitable _target; // 攻撃を受ける対象（自身）

        private void Start()
        {
            if (!TryGetComponent(out _target))
            {
                Debug.LogAssertion($"{gameObject.name} に {typeof(IHitable)}が設定されていません");
                return;
            }
            
            _target.OnHitAttack += PlayHitEffect;
        }

        /// <summary>
        /// ヒット時にエフェクトを再生する
        /// </summary>
        private void PlayHitEffect()
        {
            if(!_hitEffect) return; // nullなら以降の処理は行わない
            
            // transform.position = _hitEffect.transform.position; // TODO: 座標を設定する?
            _hitEffect.Play();
        }

        private void OnDestroy()
        {
            _target.OnHitAttack -= PlayHitEffect;
        }
    }
}
