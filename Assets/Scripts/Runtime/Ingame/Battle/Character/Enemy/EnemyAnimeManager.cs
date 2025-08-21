using UnityEngine;

namespace BeatKeeper.Runtime.Ingame.Character
{
    /// <summary>
    ///     敵のアニメーション管理システム
    /// </summary>
    public class EnemyAnimeManager : CharacterAnimeManagerB
    {
        public EnemyAnimeManager(Animator animator) : base(animator) { }

        public void KnockBack(int value = 0) => _animator?.SetTrigger(_knockBackHash);
        public void Attack() => _animator?.SetTrigger(_attackHash);
        public void PreAttack() => _animator?.SetTrigger(_preAttackHash);

        public void ChargeAttackStart() => _animator?.SetTrigger(_chargeAttackHash);
        public void ChargeAttackEnd() => _animator?.SetTrigger(_chargeAttackEndHash);
        public void ChargeAttackCancel(bool value) => _animator?.SetBool(_chargeAttackCancel, value);

        private readonly int _knockBackHash = Animator.StringToHash("KnockBack");
        private readonly int _attackHash = Animator.StringToHash("Attack");
        private readonly int _preAttackHash = Animator.StringToHash("PreAttack");
        private readonly int _chargeAttackHash = Animator.StringToHash("ChargeAttackStart");
        private readonly int _chargeAttackEndHash = Animator.StringToHash("ChargeAttackEnd");
        private readonly int _chargeAttackCancel = Animator.StringToHash("ChargeAttackCancel");
    }
}