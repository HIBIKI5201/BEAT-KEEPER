using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     敵のアニメーション管理システム
    /// </summary>
    public class EnemyAnimeManager : CharacterAnimeManagerB
    {
        public EnemyAnimeManager(Animator animator) : base(animator) { }

        public void KnockBack(bool value) => _animator?.SetBool(_knockBackHash, value);
        public void Attack() => _animator?.SetTrigger(_attackHash);
        public void PreAttack() => _animator?.SetTrigger(_preAttackHash);

        public void PreChargeAttack() => _animator?.SetTrigger(_chargeAttackHash);
        public void ChargeAttack() => _animator?.SetTrigger(_chargeAttackEndHash);

        private readonly int _knockBackHash = Animator.StringToHash("KnockBack");
        private readonly int _attackHash = Animator.StringToHash("Attack");
        private readonly int _preAttackHash = Animator.StringToHash("PreAttack");
        private readonly int _chargeAttackHash = Animator.StringToHash("ChargeAttackStart");
        private readonly int _chargeAttackEndHash = Animator.StringToHash("ChargeAttackEnd");

        private float _animeSpeed = 1f;
    }
}